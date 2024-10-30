using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Vanilla_App.Services;

namespace Vanilla.TelegramBot.Services.Bot
{
    public class BotUserCreator
    {
        public event CreatedSuccessEventHandler UpdateSuccessEvent;

        private UserContextModel _userContext;
        private readonly TelegramBotClient _botClient;
        private readonly IUserService _userService;
        private readonly ILogger _logger;

        private readonly BotUpdateUserModel _updateDataModel;

        public BotUserCreator(UserContextModel userContext, TelegramBotClient botClient, IUserService userService, ILogger logger)
        {
            _userContext = userContext;
            _botClient = botClient;
            _userService = userService;
            _logger = logger;

            // Init
            _updateDataModel = new BotUpdateUserModel();
        }

        public void EnterPoint(Telegram.BotAPI.GettingUpdates.Update update) => Router(update);

        private void Router(Telegram.BotAPI.GettingUpdates.Update update)
        {
            if (update.Message is not null) MessageAnswer(update.Message);
            else if (update.PollAnswer is not null) PollAnswer(update.PollAnswer);
            else UnexpectedInput();
        }

        private void MessageAnswer(Telegram.BotAPI.AvailableTypes.Message message)
        {
            if (!MessagePrepareHendler(message)) return;

            var text = message.Text;
            var chatId = message.Chat.Id;

            if (_updateDataModel.Nickname is null) UpdateNickname(text);
            else if (_updateDataModel.About is null) UpdateAbout(text);
            else if (_updateDataModel.Links is null) UpdateLinks(text);
            else _logger.WriteLog("Dont`t have next route", LogType.Error);

        }

        private void UpdateNickname(string text)
        {
            // Validator
            if (text.Length > 64)
            {
                MessageSendHelper("CreateNicknameValidationMess");
                return;
            }

            // Save
            _updateDataModel.Nickname = text;
            MessageSendHelper("CreateNicknameAnswerMess");
        }

        private void UpdateAbout(string text)
        {
            // Validator
            if (text.Length > 4000)
            {
                MessageSendHelper("CreateAboutValidationMess");
                return;
            }

            // Save
            _updateDataModel.About = text;
            MessageSendHelper("CreateAboutAnswerMess");
        }

        private void UpdateLinks(string text)
        {
            try
            {
                var links = FormationHelper.Links(text, _userContext);
                _updateDataModel.Links = new List<string>(links);
            }
            catch (ValidationException e)
            {
                _logger.WriteLog(e.Message, LogType.Error);
                MessageSendHelper(e.Message);
                return;
            }
            catch (Exception e)
            {
                _logger.WriteLog(e.Message, LogType.Error);
                MessageSendHelper(e.Message);
                return;
            }
        }


        private void MessageSendHelper(string resourceManagerStringName, bool isPushToQueueForDeletion = true)
        {
            //var messName = _botClient.SendMessage(_userContext.User.TelegramId, _userContext.ResourceManager.GetString(resourceManagerStringName));
            var messName = _botClient.SendMessage(_userContext.User.TelegramId, resourceManagerStringName);
            if(isPushToQueueForDeletion) _updateDataModel.SendedMessages.Add(messName.MessageId);
        }

        public void ClearMessages()
        {
            //var userProject = _userContext.CreateProjectContext;
            _botClient.DeleteMessages(_userContext.User.TelegramId, _updateDataModel.SendedMessages);
        }

        private void PollAnswer(Telegram.BotAPI.AvailableTypes.PollAnswer poll)
        {
            if (_updateDataModel.IsRadyForOrders is not null) return;

            var optionIndex = poll.OptionIds.First();
            UpdateIsRedyToWork(optionIndex);

            UpdateUser();
        }

        private void UpdateIsRedyToWork(int optionIndex)
        {
            var boolPoolAnswer = Enum.GetValues(typeof(BoolPoolAnswerEnum)).Cast<BoolPoolAnswerEnum>().ToList();

            var selectedOption = boolPoolAnswer[optionIndex];

            _updateDataModel.IsRadyForOrders = selectedOption == BoolPoolAnswerEnum.Yes;

            var messName = _botClient.SendMessage(_userContext.User.TelegramId, "CreateProkectDevelopStatusMess");
        }

        // Universal
        private void UnexpectedInput()
        {
            _logger.WriteLog("Unexpected input", LogType.Warning);
            var errorMess = _botClient.SendMessage(_userContext.User.TelegramId, _userContext.ResourceManager.GetString("UnexpectedInputMess"), parseMode: "HTML");
        }

        // Universal
        private bool MessagePrepareHendler(Telegram.BotAPI.AvailableTypes.Message message)
        {
            if (message.Text is null)
            {
                UnexpectedInput();
                return false;
            }
            if (message.ViaBot != null && message.ViaBot.Id == _botClient.GetMe().Id)
            {
                var mess = _botClient.SendMessage(message.Chat.Id, _userContext.ResourceManager.GetString("ThiIsMyMessageValidationMess"), parseMode: "HTML");
                return false;
            }
            else if (message.ViaBot != null)
            {
                UnexpectedInput();
                return false;
            }
            return true;
        }

        private void UpdateUser()
        {
            _userService.UpdateUser(_userContext.User.TelegramId, new Models.UserUpdateRequestModel
            {
                Nickname = _updateDataModel.Nickname,
                Links = _updateDataModel.Links,
                About = _updateDataModel.About,
                IsRadyForOrders = _updateDataModel.IsRadyForOrders,
            });


            // Get User data
            var userModel = _userService.GetUser(_userContext.User.UserId).Result;
            var messageContent = MessageWidgets.AboutUser(userModel);

            messageContent += "\n I successfully update your profile :3\"";


            // Clear messages
            ClearMessages();

            var messageObjWithCreatedProject = _botClient.SendMessage(_userContext.User.TelegramId, messageContent, replyMarkup: Keyboards.MainMenu(_userContext), parseMode: "HTML");
            _userContext.SendMessages.Add(messageObjWithCreatedProject.MessageId);

            UpdateSuccessEvent.Invoke(_userContext);

            // Clear "cashe"
            // ??? _updateDataModel = null; ???

        }
    }
}
