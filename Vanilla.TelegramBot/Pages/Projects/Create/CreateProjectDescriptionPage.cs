using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Abstract;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.UI;
using Vanilla_App.Models;
using static Vanilla.TelegramBot.Abstract.ActionFrame;

namespace Vanilla.TelegramBot.Pages.Projects.Create
{
    public class CreateProjectDescriptionPage : BasicPageAbstract, IPage
    {
        BotCreateProjectModel? _projectCreateModel;
        ProjectUpdateRequestModel? _projectUpdateModel;
        TelegramBotClient _botClient;
        UserContextModel _userContext;
        List<SendedMessageModel> _sendedMessages;

        public CreateProjectDescriptionPage(TelegramBotClient botClient, UserContextModel userContext, List<SendedMessageModel> sendedMessages, BotCreateProjectModel? projectCreateModel = null, ProjectUpdateRequestModel? projectUpdateModel = null) : base(botClient, userContext, sendedMessages)
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
            var messInit = _botClient.SendMessage(ChatId, _userContext.ResourceManager.GetString("CreateProkectNameAnswerMess"));
            AddMessage(messInit.MessageId, DeleteMessageMethodEnum.ClosePage);
        }

        public override void InitActions()
        {
            var textInputAction = new ActionFrame
            {
                ActionObj = new ActionDelegate(TextAction),
                Trigger = () => TextInputTrigger(),
                Validate = new ValidateDelegate(TextValidate)
            };

            AddAction(new List<ActionFrame> {
                textInputAction
            });
        }

        bool TextInputTrigger() => Helpers.ValidatorHelpers.TextValidate(CurrentUpdate);

        bool TextValidate(Telegram.BotAPI.GettingUpdates.Update update)
        {
            var messageText = update.Message.Text;
            if (messageText.Length > 4000)
            {
                ValidationError(_userContext.ResourceManager.GetString("CreateProkectDescriptionValidationMess"));
                AddMessage(update.Message.MessageId, DeleteMessageMethodEnum.NextMessage);
                return false;
            }
            return true;
        }

        void TextAction(Telegram.BotAPI.GettingUpdates.Update update)
        {
            if (_projectCreateModel is not null) _projectCreateModel.Description = update.Message.Text;
            else if (_projectUpdateModel is not null) _projectUpdateModel.Description = update.Message.Text;

            AddMessage(update.Message.MessageId, DeleteMessageMethodEnum.NextMessage);

            NextPage();
        }

    }
}
