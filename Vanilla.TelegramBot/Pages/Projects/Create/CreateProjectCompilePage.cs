using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Abstract;
using Vanilla.TelegramBot.Entityes;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages.Projects.Update;
using Vanilla.TelegramBot.UI;
using Vanilla_App.Services;
using Vanilla_App.Services.Projects;
using static Vanilla.TelegramBot.Abstract.ActionFrame;

namespace Vanilla.TelegramBot.Pages.Projects.Create
{
    public class CreateProjectCompilePage : BasicPageAbstract, IPage
    {
        BotCreateProjectModel _projectModel;
        TelegramBotClient _botClient;
        UserContextModel _userContext;
        List<SendedMessageModel> _sendedMessages;
        IProjectService _projectService;

        readonly string _deliver = " mya~ ";


        int? _initMessageId;

        public CreateProjectCompilePage(TelegramBotClient botClient, UserContextModel userContext, List<SendedMessageModel> sendedMessages, BotCreateProjectModel projectModel, IProjectService projectService) : base(botClient, userContext, sendedMessages)
        {
            _projectModel = projectModel;
            _botClient = botClient;
            _userContext = userContext;
            _sendedMessages = sendedMessages;
            _projectService = projectService;
        }

        public override void InitMessage()
        {
            var messageContent = MessageWidgets.AboutProject(_projectModel, _userContext.User, _userContext);

            if (_initMessageId is null)
            {
                var messInit = _botClient.SendMessage(_userContext.User.TelegramId, messageContent, replyMarkup: Keyboards.GetCreateProjectKeypoard(_userContext), parseMode: "HTML");
                _initMessageId = messInit.MessageId;
                AddMessage(messInit.MessageId, DeleteMessageMethodEnum.ExitFolder);
            }
            else
            {
                _botClient.EditMessageText(_userContext.User.TelegramId, messageId: (int)_initMessageId, text: messageContent, replyMarkup: Keyboards.GetCreateProjectKeypoard(_userContext), parseMode: "HTML");
            }


        }

        public override void InitActions()
        {
            var textInputAction = new ActionFrame
            {
                ActionObj = new ActionDelegate(ConfirmBtnAction),
                Trigger = () => ConfirmBtnInputTrigger(),
            };

            var updateBtnAction = new ActionFrame
            {
                ActionObj = new ActionDelegate(ConfirmUpdateBtnAction),
                Validate = new ValidateDelegate(ValidateUpdateBtnAction),
                Trigger = () => ConfirmUpdateBtnInputTrigger(),
            };

            var updateTitleBtnAction = new ActionFrame
            {
                ActionObj = new ActionDelegate(UpdateTitleBtnAction),
                Trigger = () => ConfirmUpdateTitleBtnInputTrigger(),
            };

            var updateDescriptionBtnAction = new ActionFrame
            {
                ActionObj = new ActionDelegate(UpdateDescriptionBtnAction),
                Trigger = () => ConfirmUpdateDescriptionBtnInputTrigger(),
            };

            var updateStatusBtnAction = new ActionFrame
            {
                ActionObj = new ActionDelegate(UpdateStatusBtnAction),
                Trigger = () => ConfirmUpdateStatusBtnInputTrigger(),
            };

            var updateLinksBtnAction = new ActionFrame
            {
                ActionObj = new ActionDelegate(UpdateLinksBtnAction),
                Trigger = () => ConfirmUpdateLinksBtnInputTrigger(),
            };

            AddAction(new List<ActionFrame> {
                textInputAction,
                updateBtnAction,
                updateTitleBtnAction,
                updateDescriptionBtnAction,
                updateStatusBtnAction,
                updateLinksBtnAction,
            });
        }

        bool ConfirmUpdateBtnInputTrigger() => ValidateUpdateBtnAction(CurrentUpdate);
        bool ValidateUpdateBtnAction(Telegram.BotAPI.GettingUpdates.Update update)
        {
            if (update.CallbackQuery is null) return false;
            if (update.CallbackQuery.Data is null) return false;
            if (update.CallbackQuery.Data.Length <= 0) return false;

            if (update.CallbackQuery.Data != "update") return false;

            return true;
        }

        void ConfirmUpdateBtnAction(Telegram.BotAPI.GettingUpdates.Update update)
        {
            _botClient.EditMessageReplyMarkup(chatId: ChatId, messageId: update.CallbackQuery.Message.MessageId, replyMarkup: Keyboards.GetProjectUpdateItemsKeyboard(_userContext));
        }

        bool ConfirmUpdateTitleBtnInputTrigger() => Helpers.ValidatorHelpers.CallbackBtnActionValidate(CurrentUpdate, "name");
        bool ConfirmUpdateDescriptionBtnInputTrigger() => Helpers.ValidatorHelpers.CallbackBtnActionValidate(CurrentUpdate, "description");
        bool ConfirmUpdateStatusBtnInputTrigger() => Helpers.ValidatorHelpers.CallbackBtnActionValidate(CurrentUpdate, "status");
        bool ConfirmUpdateLinksBtnInputTrigger() => Helpers.ValidatorHelpers.CallbackBtnActionValidate(CurrentUpdate, "links");

        void UpdateTitleBtnAction(Telegram.BotAPI.GettingUpdates.Update update) => ChangeFlow(new List<string> { "CreateProjectTitlePage" });
        void UpdateDescriptionBtnAction(Telegram.BotAPI.GettingUpdates.Update update) => ChangeFlow(new List<string> { "CreateProjectDescriptionPage" });
        void UpdateStatusBtnAction(Telegram.BotAPI.GettingUpdates.Update update) => ChangeFlow(new List<string> { "CreateProjectPoolPage" });
        void UpdateLinksBtnAction(Telegram.BotAPI.GettingUpdates.Update update) => ChangeFlow(new List<string> { "CreateProjectLinksPage" });

        bool ConfirmBtnInputTrigger() => Helpers.ValidatorHelpers.CallbackBtnActionValidate(CurrentUpdate, "SaveProject");

        void ConfirmBtnAction(Telegram.BotAPI.GettingUpdates.Update update)
        {
            var project = _projectService.ProjectCreateAsync(_userContext.User.UserId, new ProjectCreateRequestModel
            {
                Name = _projectModel.Name,
                Description = _projectModel.Description,
                DevelopStatus = (DevelopmentStatusEnum)_projectModel.DevelopmentStatus,
                Links = _projectModel.Links
            }).Result;

            /*//var messageContent = MessageWidgets.AboutProject(_projectModel, _userContext.User, _userContext);
            var messageContent = _userContext.ResourceManager.GetString("CreateProkectSuccessMessage");

            _botClient.EditMessageText(chatId: ChatId, messageId: _initMessageId, text: messageContent, parseMode: "HTML");

            var replyMarkup = Keyboards.GetProjectInlineOpenKeyboard(_userContext, project);
            _botClient.EditMessageReplyMarkup(chatId: ChatId, messageId: _initMessageId, replyMarkup: replyMarkup);

            AddMessage(_initMessageId, DeleteMessageMethodEnum.ExitFolder);*/

            var messageContent = _userContext.ResourceManager.GetString("CreateProkectSuccessMessage");
            var mess = _botClient.EditMessageText(chatId: ChatId, messageId: (int)_initMessageId, replyMarkup: Keyboards.GetInlineSearchProjectKeypoard(_userContext, project), text: messageContent, parseMode: "HTML");
            ChangeMessageDeleteMethod((int)_initMessageId, DeleteMessageMethodEnum.None);

           /* ChangeFlow(new List<IPage>
            {
                new ProjectInfoPage(_botClient, _userContext, _sendedMessages, project)
            });*/


            NextPage();
        }

    }
}
