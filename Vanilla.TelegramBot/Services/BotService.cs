using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Repositories;


namespace Vanilla.TelegramBot.Services
{
    public class BotService : IBotService
    {
        private readonly TelegramBotClient _botClient;
        private readonly SettingsModel _settings;
        //private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;

        private readonly string[] sites = { "*Add* own project", "Amazing project 1", "Super puper project 2", "Nyaa project 3" };
        private readonly string[] siteDescriptions =
        {
            "*Google* is a search engine",
            "Github is a git repository hosting",
            "Telegram is a messenger",
            "Wikipedia is an open wiki"
        };

        public BotService(IUserService userService) {
            _userService = userService;

            // Build a config object, using env vars and JSON providers.
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            // Get values from the config given their key and their target type.
            _settings = config.GetRequiredSection("Settings").Get<SettingsModel>();
            if (_settings == null) throw new Exception("No found setting section");

            _botClient = new TelegramBotClient(_settings.BotAccessToken);

            using CancellationTokenSource cts = new();


            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
            };

            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            Console.WriteLine("Init bot service");
        }

        async Task BotOnInlineQueryReceived(ITelegramBotClient bot, InlineQuery inlineQuery)
        {
            var results = new List<InlineQueryResult>();
            /*results.Add(new InlineQueryResultArticle(
                    "0", // we use the counter as an id for inline query results
                    sites[0], // inline query result title
                    new InputTextMessageContent(siteDescriptions[0])) // content that is submitted when the inline query result title is clicked
                );*/

            var counter = 0;
            foreach (var site in sites)
            {
                results.Add(new InlineQueryResultArticle(
                    $"{counter}", // we use the counter as an id for inline query results
                    site, // inline query result title
                    new InputTextMessageContent(siteDescriptions[counter])) // content that is submitted when the inline query result title is clicked
                );
                counter++;
            }

            await bot.AnswerInlineQueryAsync(inlineQuery.Id, results); // answer by sending the inline query result list
        }

        Task BotOnChosenInlineResultReceived(ITelegramBotClient bot, ChosenInlineResult chosenInlineResult)
        {
            if (uint.TryParse(chosenInlineResult.ResultId, out var resultId) // check if a result id is parsable and introduce variable
                && resultId < sites.Length)
            {
                Console.WriteLine($"User {chosenInlineResult.From} has selected site: {sites[resultId]}");
            }

            return Task.CompletedTask;
        }

        public async Task StartListening()
        {
            var me = await _botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.WriteLine($"https://t.me/{me.Username}");

            Console.ReadLine();
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            /*try
            {
                await (update.Type switch
                {
                    UpdateType.InlineQuery => BotOnInlineQueryReceived(_botClient, update.InlineQuery!),
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while handling {update.Type}: {ex}");
            }*/

            try
            {
                await (update.Type switch
                {
                    UpdateType.InlineQuery => BotOnInlineQueryReceived(_botClient, update.InlineQuery!),
                    UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(_botClient, update.ChosenInlineResult!),
                });
            }
            catch { }


            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Message is not { } message)
                return;
            // Only process text messages
            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;

            UserModel? userModel;

            // If user don`t exist, create new user
            try
            {
                userModel = await _userService.SignInUser(chatId);
            }
            catch (Exception ex)
            {
                userModel = await _userService.RegisterUser(new UserRegisterModel
                {
                    TelegramId = chatId,
                    Username = message.Chat.Username,
                    FirstName = message.Chat.FirstName,
                    LastName = message.Chat.LastName
                });
            }   

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            // Echo received message text
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "You said:\n" + messageText,
                cancellationToken: cancellationToken);
        }
        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
