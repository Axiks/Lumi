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

            var messInit = _botClient.SendMessage(userContext.User.TelegramId, userContext.ResourceManager.GetString("CreateProjectInitMess"), replyMarkup: Keyboards.CannelKeyboard(userContext));
            userContext.CreateProjectContext.SendedMessages.Add(messInit.MessageId);
        }

        public void EnterPoint(Telegram.BotAPI.GettingUpdates.Update update)
        {
            var userProject = _userContext.CreateProjectContext;

            if (update.Message is not null)
            {
                if (!MessagePrepareHendler(update)) return;
                if (update.Message.Text is null)
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
                        var messValidation = _botClient.SendMessage(chatId, _userContext.ResourceManager.GetString("CreateProkectNameValidationMess"), parseMode: "HTML");
                        userProject.SendedMessages.Add(messValidation.MessageId);
                        return;
                    }

                    userProject.Name = messageText;

                    var messName = _botClient.SendMessage(chatId, _userContext.ResourceManager.GetString("CreateProkectNameAnswerMess"));
                    userProject.SendedMessages.Add(messName.MessageId);
                    return;
                }

                if (userProject.Description is null)
                {
                    if (messageText.Length > 4000)
                    {
                        var messValidation = _botClient.SendMessage(chatId, _userContext.ResourceManager.GetString("CreateProkectDescriptionValidationMess"), parseMode: "HTML");
                        userProject.SendedMessages.Add(messValidation.MessageId);
                        return;
                    }

                    userProject.Description = messageText;



                    var pollArgs = MessageWidgets.GeneratePull(chatId, _userContext);
                    var sendedMessage = _botClient.SendPoll(pollArgs);

                    userProject.PollIdDevelopmentStatus = sendedMessage.Poll.Id;

                    userProject.SendedMessages.Add(sendedMessage.MessageId);

                    return;
                }

                if (userProject.DevelopmentStatus is null)
                {
                    var messValidation = _botClient.SendMessage(chatId, _userContext.ResourceManager.GetString("CreateProkectDevelopStatusValidationMess"), parseMode: "HTML");
                    userProject.SendedMessages.Add(messValidation.MessageId);
                    return;
                }

                if (userProject.Links is null)
                {
                    try
                    {
                        var links = FormationHelper.Links(messageText, _userContext);
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
                    catch (Exception e)
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
            else if (update.ChosenInlineResult is not null)
            {
                // fix
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
            var errorMess = _botClient.SendMessage(_userContext.User.TelegramId, _userContext.ResourceManager.GetString("UnexpectedInputMess"), parseMode: "HTML");
            userProject.SendedMessages.Add(errorMess.MessageId);
        }

        private bool MessagePrepareHendler(Telegram.BotAPI.GettingUpdates.Update botUpdate)
        {
            if (botUpdate.Message is null) return false;

            var userProject = _userContext.CreateProjectContext;
            userProject.SendedMessages.Add(botUpdate.Message.MessageId);

            if (botUpdate.Message.ViaBot != null && botUpdate.Message.ViaBot.Id == _botClient.GetMe().Id)
            {
                var mess = _botClient.SendMessage(botUpdate.Message.Chat.Id, _userContext.ResourceManager.GetString("ThiIsMyMessageValidationMess"), parseMode: "HTML");
                userProject.SendedMessages.Add(mess.MessageId);
                return false;
            }
            else if (botUpdate.Message.ViaBot != null)
            {
                UnexpectedInput();
                return false;
            }
            return true;
        }

        private void PollHendler(Telegram.BotAPI.GettingUpdates.Update update)
        {
            if (update.PollAnswer is null) return;

            var userProject = _userContext.CreateProjectContext;
            if (userProject.DevelopmentStatus is not null) return;

            var poll = update.PollAnswer;

            var optionIndex = poll.OptionIds.First();
            var statusAsList = Enum.GetValues(typeof(DevelopmentStatusEnum)).Cast<DevelopmentStatusEnum>().ToList();

            var selectedOption = statusAsList[optionIndex];

            userProject.DevelopmentStatus = selectedOption;



            var messPoll = _botClient.SendMessage(poll.User.Id, _userContext.ResourceManager.GetString("CreateProkectDevelopStatusMess"), parseMode: "HTML", linkPreviewOptions: new LinkPreviewOptions() { IsDisabled = true });
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
                DevelopStatus = (DevelopmentStatusEnum)userProject.DevelopmentStatus,
                Links = userProject.Links
            }).Result;

            //_botClient.SendMessage(_userContext.User.TelegramId, "I successfully added your project :3");

            var messageContent = MessageWidgets.AboutProject(project, _userContext.User, _userContext);
            messageContent += _userContext.ResourceManager.GetString("CreateProkectSuccessMessage");

            // Clear messages
            ClearMessages();

            var replyMarkuppp = GetProjectInlineOpenKeyboard(project, _userContext);
            //echo information about project
            //_botClient.SendMessage(_userContext.User.TelegramId, messageContent, replyMarkup: Keyboards.MainMenu(), parseMode: "HTML");
            var messageObjWithCreatedProject = _botClient.SendMessage(_userContext.User.TelegramId, messageContent, replyMarkup: replyMarkuppp, parseMode: "HTML");
            _userContext.SendMessages.Add(messageObjWithCreatedProject.MessageId);

            var messageObjWithMenu = _botClient.SendMessage(_userContext.User.TelegramId, _userContext.ResourceManager.GetString("MainMenuSendMes"), replyMarkup: Keyboards.MainMenu(_userContext));
            _userContext.SendMessages.Add(messageObjWithMenu.MessageId);

            CreatedSuccessEvent.Invoke(_userContext);
            // Clear "cashe"
            _userContext.CreateProjectContext = null;
        }

        public void ClearMessages()
        {
            var userProject = _userContext.CreateProjectContext;
            _botClient.DeleteMessages(_userContext.User.TelegramId, userProject.SendedMessages);
        }

        private InlineKeyboardMarkup GetProjectInlineOpenKeyboard(ProjectModel projectModel, UserContextModel userContext)
        {
            var SearchProjectBtn = new InlineKeyboardButton(text: _userContext.ResourceManager.GetString("FindThisProjectBtn"));
            SearchProjectBtn.SwitchInlineQueryCurrentChat = projectModel.Name;

            string deliver = " mya~ ";

            var updateBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("UpdateBtn"));
            var deleteBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("DeleteBtn"));
            updateBtn.CallbackData = "update" + deliver + projectModel.Id;
            deleteBtn.CallbackData = "delete" + deliver + projectModel.Id;

            var replyMarkuppp = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                    new InlineKeyboardButton[]{
                        updateBtn,
                        deleteBtn
                    },
                    new InlineKeyboardButton[]{
                        SearchProjectBtn
                    },
                }
            );

            return replyMarkuppp;
        }

    }
}
