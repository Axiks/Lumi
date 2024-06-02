using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Entityes;
using Vanilla.TelegramBot.Helpers;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.UI;
using Vanilla_App.Interfaces;
using Vanilla_App.Models;

namespace Vanilla.TelegramBot.Services.Bot
{
    public delegate void CreatedSuccessEventHandler(UserContextModel userContext);
    public class BotProjectCreator
    {
        public event CreatedSuccessEventHandler CreatedSuccessEvent;

        private readonly UserContextModel _userContext;
        private readonly TelegramBotClient _botClient;
        private readonly IProjectService _projectService;

        private readonly ILogger _logger;

        public BotProjectCreator(UserContextModel userContext, TelegramBotClient botClient, IProjectService projectService, ILogger logger)
        {
            _userContext = userContext;
            _botClient = botClient;
            _projectService = projectService;
            _userContext.CreateProjectContext = new BotCreateProjectModel(userContext.User.UserId, userContext.User.TelegramId);
            _logger = logger;

            var messInit = _botClient.SendMessage(userContext.User.TelegramId, "What is the name of your wonderful project?", replyMarkup: Keyboards.CannelKeyboard());
            userContext.CreateProjectContext.SendedMessages.Add(messInit.MessageId);
        }

        public void EnterPoint(Telegram.BotAPI.GettingUpdates.Update update)
        {
            var userProject = _userContext.CreateProjectContext;

            if (update.Message is not null)
            {
                if (!MessagePrepareHendler(update)) return;
                if(update.Message.Text is null)
                {
                    UnexpectedInput();
                    return;
                }

                    var messageText = update.Message.Text;
                var chatId = update.Message.Chat.Id;

                // Save name
                if (userProject.Name is null)
                {
                    if (messageText.Length > 64)
                    {
                        var messValidation = _botClient.SendMessage(chatId, "Wow, the name of your project is as catchy as the names of some anime!\r\n\r\nBut unfortunately, I cannot accept it\r\nThe name must not exceed 64 characters", parseMode: "HTML");
                        userProject.SendedMessages.Add(messValidation.MessageId);
                        return;
                    }

                    userProject.Name = messageText;

                    var messName = _botClient.SendMessage(chatId, "Tell us more about it");
                    userProject.SendedMessages.Add(messName.MessageId);
                    return;
                }

                if (userProject.Description is null)
                {
                    if (messageText.Length > 4000)
                    {
                        var messValidation = _botClient.SendMessage(chatId, "Wow Wow I see huge ambitions here, but alas, I can't fit them into one calf\r\n\r\nDescribe your project more concisely. The maximum I can write is 4000 characters", parseMode: "HTML");
                        userProject.SendedMessages.Add(messValidation.MessageId);
                        return;
                    }

                    userProject.Description = messageText;

                    var pollArgs = MessageWidgets.GeneratePull(chatId);
                    var sendedMessage = _botClient.SendPoll(pollArgs);

                    userProject.PollIdDevelopmentStatus = sendedMessage.Poll.Id;

                    userProject.SendedMessages.Add(sendedMessage.MessageId);

                    return;
                }

                if (userProject.DevelopStatus is null)
                {
                    var messValidation = _botClient.SendMessage(chatId, "Whoops!\n\nYou must choose the correct answer, not write to me!", parseMode: "HTML");
                    userProject.SendedMessages.Add(messValidation.MessageId);
                    return;
                }

                if (userProject.Links is null)
                {
                    try
                    {
                        var links =  FormationHelper.Links(messageText);
                        userProject.Links = new List<string>(links);
                        AddProject();
                    }
                    catch (ValidationException e)
                    {
                        _logger.WriteLog(e.Message, LogType.Error);
                        var errorMess = _botClient.SendMessage(chatId, e.Message, parseMode: "HTML");
                        userProject.SendedMessages.Add(errorMess.MessageId);
                        return;
                    }
                    catch(Exception  e)
                    {
                        _logger.WriteLog(e.Message, LogType.Error);
                        var errorMess = _botClient.SendMessage(chatId, e.Message, parseMode: "HTML");
                        userProject.SendedMessages.Add(errorMess.MessageId);
                        return;
                    }

                }
            }
            else if (update.PollAnswer is not null)
            {
                PollHendler(update);
            }
            else
            {
                UnexpectedInput();
            }


        }

        private void UnexpectedInput()
        {
            var userProject = _userContext.CreateProjectContext;

            _logger.WriteLog("Unexpected input", LogType.Warning);
            var errorMess = _botClient.SendMessage(_userContext.User.TelegramId, "Oooh\r\nHow nice of you to send this, but it's not what I expected ;(", parseMode: "HTML");
            userProject.SendedMessages.Add(errorMess.MessageId);
        }

        private bool MessagePrepareHendler(Telegram.BotAPI.GettingUpdates.Update botUpdate)
        {
            if (botUpdate.Message is null) return false;

            var userProject = _userContext.CreateProjectContext;
            userProject.SendedMessages.Add(botUpdate.Message.MessageId);

            if (botUpdate.Message.ViaBot != null && botUpdate.Message.ViaBot.Id == _botClient.GetMe().Id)
            {
                var mess = _botClient.SendMessage(botUpdate.Message.Chat.Id, "Munch\r\nThis is my message!!! \n<b>I won't miss it</b>\n\nWrite it yourself", parseMode: "HTML");
                userProject.SendedMessages.Add(mess.MessageId);
                return false;
            }
            return true;
        }

        private void PollHendler(Telegram.BotAPI.GettingUpdates.Update update)
        {
            if(update.PollAnswer is null) return;

            var userProject = _userContext.CreateProjectContext;
            if (userProject.DevelopStatus is not null) return;

            var poll = update.PollAnswer;

            var optionIndex = poll.OptionIds.First();
            var statusAsList = Enum.GetValues(typeof(DevelopmentStatusEnum)).Cast<DevelopmentStatusEnum>().ToList();

            var selectedOption = statusAsList[optionIndex];

            userProject.DevelopStatus = selectedOption;
            var messPoll = _botClient.SendMessage(poll.User.Id, "List the link to this project via comma");
            userProject.SendedMessages.Add(messPoll.MessageId);
            return;
        }

        private void AddProject()
        {
            var userProject = _userContext.CreateProjectContext;

            var project = _projectService.ProjectCreateAsync(_userContext.User.UserId, new ProjectCreateRequestModel
            {
                Name = userProject.Name,
                Description = userProject.Description,
                DevelopStatus = (DevelopmentStatusEnum)userProject.DevelopStatus,
                Links = userProject.Links
            }).Result;

            //_botClient.SendMessage(_userContext.User.TelegramId, "I successfully added your project :3");
            
            var messageContent = MessageWidgets.AboutProject(project, _userContext.User);
            messageContent += "\n\n <b>🌟 I successfully added your project :3</b>";

            // Clear messages
            ClearMessages();

            //echo information about project
            _botClient.SendMessage(_userContext.User.TelegramId, messageContent, replyMarkup: Keyboards.MainMenu(), parseMode: "HTML");

            CreatedSuccessEvent.Invoke(_userContext);
            // Clear "cashe"
            _userContext.CreateProjectContext = null;
        }

        public void ClearMessages()
        {
            var userProject = _userContext.CreateProjectContext;
            _botClient.DeleteMessages(_userContext.User.TelegramId, userProject.SendedMessages);
        }

    }
}
