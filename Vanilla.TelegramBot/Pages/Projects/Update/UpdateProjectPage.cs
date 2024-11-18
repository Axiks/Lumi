using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Abstract;
using Vanilla.TelegramBot.Entityes;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages.Projects.Create;
using Vanilla.TelegramBot.UI;
using Vanilla_App.Interfaces;
using Vanilla_App.Models;
using static Vanilla.TelegramBot.Abstract.ActionFrame;

namespace Vanilla.TelegramBot.Pages.Projects.Update
{
    public class TESTProjectInfoPage : BasicPageAbstract, IPage, IPageChangeFlowExtension
    {
        ProjectModel _projectModel;
        TelegramBotClient _botClient;
        UserContextModel _userContext;
        List<SendedMessageModel> _sendedMessages;
        IProjectService _projectService;

        ProjectUpdateRequestModel _projectUpdateRequestModel;

        int? _initMessageId;

        public TESTProjectInfoPage(TelegramBotClient botClient, UserContextModel userContext, List<SendedMessageModel> sendedMessages, ProjectModel projectModel, int? initMessage = null) : base(botClient, userContext, sendedMessages)
        {
            _projectModel = projectModel;
            _botClient = botClient;
            _userContext = userContext;
            _sendedMessages = sendedMessages;
            _initMessageId = initMessage;

            _projectUpdateRequestModel = new ProjectUpdateRequestModel {
                Id = _projectModel.Id,
                Name = _projectModel.Name,
                Description = _projectModel.Description,
                ProjectRequest  =   _projectModel.DevelopmentStatus,
                Links = _projectModel.Links,
            };
        }

        public override void InitMessage()
        {
    /*        var messageContent = MessageWidgets.AboutProject(_projectModel, _userContext.User, _userContext);

            var replyMarkup = Keyboards.GetProjectUpdateItemsKeyboard(_userContext);


            if (_initMessageId is null)
            {
                var messInit = _botClient.SendMessage(chatId: ChatId, replyMarkup: replyMarkup, text: messageContent, parseMode: "HTML");
                _initMessageId = messInit.MessageId;
                AddMessage(messInit.MessageId, DeleteMessageMethodEnum.ExitFolder);
            }
            else _botClient.EditMessageReplyMarkup(chatId: ChatId, messageId: _initMessageId ?? 0, replyMarkup: replyMarkup);
*/
            //NextPage();
        }

        public override void InitActions()
        {

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
                updateTitleBtnAction,
                updateDescriptionBtnAction,
                updateLinksBtnAction,
                updateStatusBtnAction
            });
        }

        bool ConfirmUpdateTitleBtnInputTrigger() => Helpers.ValidatorHelpers.CallbackBtnActionValidate(CurrentUpdate, "name");
        bool ConfirmUpdateDescriptionBtnInputTrigger() => Helpers.ValidatorHelpers.CallbackBtnActionValidate(CurrentUpdate, "description");
        bool ConfirmUpdateStatusBtnInputTrigger() => Helpers.ValidatorHelpers.CallbackBtnActionValidate(CurrentUpdate, "status");
        bool ConfirmUpdateLinksBtnInputTrigger() => Helpers.ValidatorHelpers.CallbackBtnActionValidate(CurrentUpdate, "links");

        void UpdateTitleBtnAction(Telegram.BotAPI.GettingUpdates.Update update)
        {
            ChangeFlow(new List<IPage> { new CreateProjectTitlePage(_botClient, _userContext, _sendedMessages, projectUpdateModel: _projectUpdateRequestModel) });
        }

        void UpdateDescriptionBtnAction(Telegram.BotAPI.GettingUpdates.Update update)
        {
            ChangeFlow(new List<IPage> { new CreateProjectDescriptionPage(_botClient, _userContext, _sendedMessages, projectUpdateModel: _projectUpdateRequestModel) });
        }

        void UpdateStatusBtnAction(Telegram.BotAPI.GettingUpdates.Update update)
        {
            ChangeFlow(new List<IPage> { new CreateProjectPoolPage(_botClient, _userContext, _sendedMessages, projectUpdateModel: _projectUpdateRequestModel) });
        }

        void UpdateLinksBtnAction(Telegram.BotAPI.GettingUpdates.Update update)
        {
            ChangeFlow(new List<IPage> { new CreateProjectLinksPage(_botClient, _userContext, _sendedMessages, projectUpdateModel: _projectUpdateRequestModel) });
        }

    }
}
