using System.ComponentModel.DataAnnotations;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.TelegramBot.Helpers;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Pages.UpdateUser
{
    internal class UpdateUserLinksPage : IPage
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
        readonly TelegramBotClient _botClient;
        readonly UserContextModel _userContext;
        readonly List<int> _sendMessages;
        BotUpdateUserModel _dataContext;

        readonly string InitMessage = "Перерахуйте, через кому, посилання на влсасні проекти *Навести приклад*";

        public UpdateUserLinksPage(TelegramBotClient botClient, UserContextModel userContext, List<int> sendMessages, BotUpdateUserModel dataContext)
        {
            _botClient = botClient;
            _userContext = userContext;
            _sendMessages = sendMessages;
            _dataContext = dataContext;
        }

        void IPage.SendInitMessage() => MessageSendHelper(InitMessage);

        void IPage.InputHendler(Update update)
        {
            if (!ValidateInputType(update)) return;
            if (!ValidateInputData(update)) return;

            Action(update);
        }

        bool ValidateInputType(Update update)
        {
            if (update.Message is null || update.Message.Text is null)
            {
                ValidationErrorEvent.Invoke("Не те що очікувала. Введи текст!");
                return false;
            }
            return true;
        }

        bool ValidateInputData(Update update)
        {
            if (update.Message!.Text!.Length > 4000)
            {
                ValidationErrorEvent.Invoke("Ойй ой ой\n\n Посилання, у сумні, не можуть бути таке довше за 4000 символи.");
                return false;
            }

            try
            {
                FormationHelper.Links(update.Message!.Text!, _userContext);
            }
            catch (ValidationException e)
            {
                ValidationErrorEvent.Invoke("Ойй ой ой\n\n Я на розпізнанала усі посилання \n Далі інфа про те як його правильно писати");
                return false;
            }
            catch (Exception e)
            {
                throw e;
            }

            return true;
        }

        void Action(Update update) => _dataContext.Links = new List<string>(FormationHelper.Links(update.Message!.Text!, _userContext));

        void MessageSendHelper(string text)
        {
            var mess = _botClient.SendMessage(_userContext.User.TelegramId, text);
            _sendMessages.Add(mess.MessageId);
        }
    }
}
