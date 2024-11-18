using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.UI;
using Vanilla_App.Models;
using static Vanilla.TelegramBot.Pages.ActionFrame;

namespace Vanilla.TelegramBot.Pages.Projects.Create
{
    public class CreateProjectTitlePage : BasicPageAbstract, IPage
    {
        BotCreateProjectModel? _projectCreateModel;
        ProjectUpdateRequestModel? _projectUpdateModel;
        TelegramBotClient _botClient;
        UserContextModel _userContext;
        List<SendedMessageModel> _sendedMessages;

        public CreateProjectTitlePage(TelegramBotClient botClient, UserContextModel userContext, List<SendedMessageModel> sendedMessages, BotCreateProjectModel? projectCreateModel = null, ProjectUpdateRequestModel? projectUpdateModel = null) : base(botClient, userContext, sendedMessages)
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
            var reply = new ForceReply();
            reply.InputFieldPlaceholder = "Write your name";

            var messInit = _botClient.SendMessage(ChatId, _userContext.ResourceManager.GetString("CreateProjectInitMess"));
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
            if (messageText.Length > 64)
            {
                ValidationError(_userContext.ResourceManager.GetString("CreateProkectNameValidationMess"));
                AddMessage(update.Message.MessageId, DeleteMessageMethodEnum.NextMessage);
                return false;
            }
            return true;
        }

        void TextAction(Telegram.BotAPI.GettingUpdates.Update update)
        {
            if(_projectCreateModel is not null) _projectCreateModel.Name = update.Message.Text;
            else if(_projectUpdateModel is not null) _projectUpdateModel.Name = update.Message.Text;
            
            AddMessage(update.Message.MessageId, DeleteMessageMethodEnum.NextMessage);

            NextPage();
        }

    }
}
