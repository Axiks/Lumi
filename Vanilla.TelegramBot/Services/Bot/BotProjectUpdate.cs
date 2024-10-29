using System.ComponentModel.DataAnnotations;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
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
    public delegate void UpdatedSuccessEventHandler(UserContextModel userContext);
    public enum Command
    {
        name,
        description,
        status,
        links
    }

    public class BotProjectUpdate
    {
        public event UpdatedSuccessEventHandler UpdatedSuccessEvent;

        private UserContextModel _userContext;
        private readonly TelegramBotClient _botClient;
        private readonly IProjectService _projectService;
        public readonly Command command;
        public ProjectModel? projectModel;
        //public readonly SelectedItem selectedItem;
        private readonly ILogger _logger;
        private List<int> _sendedMessagesId;
        public readonly int updateMessageId;


        public BotProjectUpdate(UserContextModel userContext, TelegramBotClient botClient, IProjectService projectService, Telegram.BotAPI.GettingUpdates.Update update, Guid projectId, Command command, ILogger logger, int updateMessageId)
        {
            _userContext = userContext;
            _botClient = botClient;
            _projectService = projectService;
            _logger = logger;
            this.command = command;
            this.updateMessageId = updateMessageId;


            _sendedMessagesId = new List<int>();

            projectModel = _projectService.ProjectGetAsync(projectId).Result;

            // Welcome enterpoint

            //pin message
            _botClient.PinChatMessage(chatId: _userContext.User.TelegramId, messageId: updateMessageId);
            // init message
            InitMessage(update);
        }

        private void InitMessage(Telegram.BotAPI.GettingUpdates.Update update)
        {
            Message? sendedMessage = null;
            if (command == Command.name)
            {
                sendedMessage = _botClient.SendMessage(_userContext.User.TelegramId, _userContext.ResourceManager.GetString("WhatValueToReplace"), replyMarkup: Keyboards.CannelKeyboard(_userContext));
            }
            else if (command == Command.description)
            {
                sendedMessage = _botClient.SendMessage(_userContext.User.TelegramId, _userContext.ResourceManager.GetString("WhatValueToReplace"), replyMarkup: Keyboards.CannelKeyboard(_userContext));
            }
            else if (command == Command.status)
            {
                // Generate new pool
                var pollArgs = MessageWidgets.GeneratePull(_userContext.User.TelegramId, _userContext);
                pollArgs.ReplyMarkup = Keyboards.CannelKeyboard(_userContext);
                sendedMessage = _botClient.SendPoll(pollArgs);
            }
            else if (command == Command.links)
            {
                sendedMessage = _botClient.SendMessage(_userContext.User.TelegramId, _userContext.ResourceManager.GetString("WhatValueToReplace"), replyMarkup: Keyboards.CannelKeyboard(_userContext));
            }

            if (sendedMessage is not null)
            {
                FocusOnTheUpdateMessage(updateMessageId);
                _sendedMessagesId.Add(sendedMessage.MessageId);
            }
        }

        public void FocusOnTheUpdateMessage(int updateMessageId)
        {
            var messagesToDelete = _userContext.SendMessages.Where(x => x != updateMessageId);
            if (messagesToDelete.Count() == 0) return;

            _botClient.DeleteMessages(_userContext.User.TelegramId, messagesToDelete);
            _userContext.SendMessages.RemoveAll(x => x != updateMessageId);
        }

        public void EnterPoint(Telegram.BotAPI.GettingUpdates.Update update)
        {
            if (update.Message is not null)
            {
                if (!MessagePrepareHendler(update)) return;

                _sendedMessagesId.Add(update.Message.MessageId);

                var text = update.Message.Text;
                var updateModel = new Vanilla_App.Models.ProjectUpdateRequestModel { Id = projectModel.Id };
                switch (command)
                {
                    case Command.name:
                        if (text.Length > 64)
                        {
                            var messValidation = _botClient.SendMessage(_userContext.User.TelegramId, _userContext.ResourceManager.GetString("UpdateProjectNameValidation"), parseMode: "HTML");
                            _sendedMessagesId.Add(messValidation.MessageId);
                            return;
                        }
                        updateModel.Name = text;
                        break;
                    case Command.description:
                        if (text.Length > 4000)
                        {
                            var messValidation = _botClient.SendMessage(_userContext.User.TelegramId, _userContext.ResourceManager.GetString("UpdateProjectDescriptionValidation"), parseMode: "HTML");
                            _sendedMessagesId.Add(messValidation.MessageId);
                            return;
                        }
                        updateModel.Description = text;
                        break;
                    case Command.links:
                        try
                        {
                            updateModel.Links = FormationHelper.Links(text, _userContext);
                        }
                        catch (ValidationException e)
                        {
                            _logger.WriteLog(e.Message, LogType.Error);
                            var sendedMessage = _botClient.SendMessage(_userContext.User.TelegramId, e.Message, parseMode: "HTML");
                            _sendedMessagesId.Add(sendedMessage.MessageId);
                            return;
                        }
                        catch (Exception e)
                        {
                            _logger.WriteLog(e.Message, LogType.Error);
                            var sendedMessage = _botClient.SendMessage(_userContext.User.TelegramId, e.Message, parseMode: "HTML");
                            _sendedMessagesId.Add(sendedMessage.MessageId);
                            return;
                        }
                        break;
                    default:
                        UnexpectedInput();
                        return;
                }
                _projectService.ProjectUpdateAsync(updateModel);
                SuccessUpdated(update);
            }
            else if (update.PollAnswer is not null)
            {
                if (command != Command.status)
                {
                    _logger.WriteLog("Acccaes dainen to poll", LogType.Error);
                    return;
                };

                var poll = update.PollAnswer;

                var optionIndex = poll.OptionIds.First();
                var statusAsList = Enum.GetValues(typeof(DevelopmentStatusEnum)).Cast<DevelopmentStatusEnum>().ToList();

                var selectedOption = statusAsList[optionIndex];

                var updateModel = new Vanilla_App.Models.ProjectUpdateRequestModel { Id = projectModel.Id };
                updateModel.ProjectRequest = selectedOption;
                _projectService.ProjectUpdateAsync(updateModel);
                SuccessUpdated(update);
            }
            else if (update.ChosenInlineResult is not null)
            {
                // fix
            }
            else
            {
                UnexpectedInput();
            }
        }

        private void SuccessUpdated(Telegram.BotAPI.GettingUpdates.Update update)
        {
            ClearMessages();

            //forming project card
            var mwssageObj = _botClient.SendMessage(_userContext.User.TelegramId, _userContext.ResourceManager.GetString("UpdateProjectSuccess"));
            _userContext.SendMessages.Add(mwssageObj.MessageId);

            UpdatedSuccessEvent.Invoke(_userContext);
        }

        private bool MessagePrepareHendler(Telegram.BotAPI.GettingUpdates.Update botUpdate)
        {
            if (botUpdate.Message is null) return false;

            if (botUpdate.Message.PinnedMessage is not null)
            {
                var messageId = botUpdate.Message.MessageId;
                _sendedMessagesId.Add(messageId);
                return false;
            }

            if (command == Command.status)
            {
                _logger.WriteLog("Acccaes dainen to poll", LogType.Error);
                _sendedMessagesId.Add(botUpdate.Message.MessageId);

                var messValidation = _botClient.SendMessage(_userContext.User.TelegramId, _userContext.ResourceManager.GetString("UpdateProjectPoolValidation"), parseMode: "HTML");
                _sendedMessagesId.Add(messValidation.MessageId);
                return false;
            };

            if (botUpdate.Message.ViaBot != null && botUpdate.Message.ViaBot.Id == _botClient.GetMe().Id)
            {
                _sendedMessagesId.Add(botUpdate.Message.MessageId);
                var mess = _botClient.SendMessage(botUpdate.Message.Chat.Id, _userContext.ResourceManager.GetString("ThiIsMyMessageValidationMess"), parseMode: "HTML");
                _sendedMessagesId.Add(mess.MessageId);
                return false;
            }
            else if (botUpdate.Message.ViaBot != null)
            {
                _sendedMessagesId.Add(botUpdate.Message.MessageId);
                UnexpectedInput();
                return false;
            }
            else if (botUpdate.Message.Text is null)
            {
                _sendedMessagesId.Add(botUpdate.Message.MessageId);
                UnexpectedInput();
                return false;
            }

            return true;
        }

        private void UnexpectedInput()
        {
            _logger.WriteLog("UnexpectedInpu", LogType.Warning);
            var errorMess = _botClient.SendMessage(_userContext.User.TelegramId, _userContext.ResourceManager.GetString("UnexpectedInputMess"), parseMode: "HTML");
            _sendedMessagesId.Add(errorMess.MessageId);
        }
        public void ClearMessages()
        {
            // unpin update message
            _botClient.UnpinChatMessage(chatId: _userContext.User.TelegramId, messageId: updateMessageId);

            _botClient.DeleteMessages(messageIds: _sendedMessagesId, chatId: _userContext.User.TelegramId);
        }

    }
}
