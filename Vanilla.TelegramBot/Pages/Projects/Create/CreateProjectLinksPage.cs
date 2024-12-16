using System.ComponentModel.DataAnnotations;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Abstract;
using Vanilla.TelegramBot.Helpers;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.UI;
using Vanilla_App.Services.Projects;
using static Vanilla.TelegramBot.Abstract.ActionFrame;

namespace Vanilla.TelegramBot.Pages.Projects.Create
{
    public class CreateProjectLinksPage : BasicPageAbstract, IPage
    {
        BotCreateProjectModel? _projectCreateModel;
        ProjectUpdateRequestModel? _projectUpdateModel; TelegramBotClient _botClient;
        UserContextModel _userContext;
        List<SendedMessageModel> _sendedMessages;

        public CreateProjectLinksPage(TelegramBotClient botClient, UserContextModel userContext, List<SendedMessageModel> sendedMessages, BotCreateProjectModel? projectCreateModel = null, ProjectUpdateRequestModel? projectUpdateModel = null) : base(botClient, userContext, sendedMessages)
        {
            _projectCreateModel = projectCreateModel;
            _projectUpdateModel = projectUpdateModel;
            _botClient = botClient;
            _userContext = userContext;
            _sendedMessages = sendedMessages;
        }

        public override void InitMessage()
        {
            var keyboard = Keyboards.GetPassKeypoard(_userContext);
            var messInit = _botClient.SendMessage(ChatId, _userContext.ResourceManager.GetString("CreateProkectDevelopStatusMess"), replyMarkup: keyboard, parseMode: "HTML", linkPreviewOptions: new LinkPreviewOptions() { IsDisabled = true });
            AddMessage(messInit.MessageId, DeleteMessageMethodEnum.ClosePage);
        }

        public override void InitActions()
        {
            var textInputAction = new ActionFrame
            {
                ActionObj = new ActionDelegate(TextAction),
                Trigger = () => TextInputTrigger(),
                Validate = new ValidateDelegate(LinksValidate)
            };

            AddAction(new List<ActionFrame> {
                textInputAction
            });
        }

        bool TextInputTrigger() => ValidatorHelpers.TextValidate(CurrentUpdate);

        bool LinksValidate(Telegram.BotAPI.GettingUpdates.Update update)
        {
            var messageText = update.Message.Text;

            try
            {
                var links = FormationHelper.Links(messageText, _userContext);
                AddMessage(update.Message.MessageId, DeleteMessageMethodEnum.ClosePage);

            }
            catch (ValidationException e)
            {
                ValidationError(e.Message);
                return false;
            }

            return true;
        }

        void TextAction(Telegram.BotAPI.GettingUpdates.Update update)
        {
            var messageText = update.Message.Text;
            var links = FormationHelper.Links(messageText, _userContext);

            if (_projectCreateModel is not null) _projectCreateModel.Links = new List<string>(links);
            else if (_projectUpdateModel is not null) _projectUpdateModel.Links = new List<string>(links);

            NextPage();
        }

    }
}
