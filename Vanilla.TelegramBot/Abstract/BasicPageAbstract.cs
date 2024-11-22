using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.UI;
using static Vanilla.TelegramBot.Abstract.ActionFrame;

namespace Vanilla.TelegramBot.Abstract
{
    delegate Exception? InputValidator();
    public abstract class BasicPageAbstract(TelegramBotClient botClient, UserContextModel userContext, List<SendedMessageModel> sendedMessages) : IPage, IPageChangeFlowExtension
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
        public event ChangePagesFlowEventHandler? ChangePagesFlowPagesEvent;
        public event CompliteHandler? CompliteEvent;
        public event ChangePagesFlowByPagesEventHandler? ChangePagesFlowByPagesPagesEvent;

        public long ChatId = userContext.User.TelegramId;

        List<ActionFrame> _inputActionsList = new List<ActionFrame>();

        bool isFirstInput = true;

        public Update CurrentUpdate;

        void IPage.SendInitMessage()
        {
            InitMessage();
        }

        void IPage.InputHendler(Update update)
        {
            CurrentUpdate = update;
            //if (Validators() is false) return;
            if (isFirstInput is true) PrepareBasicActions();
            if (isFirstInput is true) InitActions();
            isFirstInput = false;

            ActionInput(update);
        }

        public abstract void InitMessage();
        public abstract void InitActions();

        void ActionInput(Update update)
        {
            bool isProblemWithValidation = false;

            if (_inputActionsList is null) return;

            foreach (var action in _inputActionsList)
            {

                if (action.Trigger() is true)
                {
                    bool isValidate = action.Validate is not null ? action.Validate(update) : true;
                    if (isValidate is false) {
                        isProblemWithValidation = true;
                        continue;
                    }

                    action.ActionObj(update);
                    return;
                }
            }

            if(isProblemWithValidation == false) ActionDontFound();
            //throw new Exception("No actions found");
        }

        void ActionDontFound()
        {
            if (CurrentUpdate.Message is not null) AddMessage(CurrentUpdate.Message.MessageId, DeleteMessageMethodEnum.NextAction);

            var mess = botClient.SendMessage(ChatId, "Action not recognized.\nFollow the instructions above or cancel the current operation.", replyMarkup: Keyboards.CannelInlineKeyboard(userContext));
            AddMessage(mess.MessageId, DeleteMessageMethodEnum.NextAction);
        }

        public void AddMessage(int messageId, DeleteMessageMethodEnum deleteMessageMethodEnum) => sendedMessages.Add(new SendedMessageModel(messageId, deleteMessageMethodEnum));
        public void ChangeMessageDeleteMethod(int messageId, DeleteMessageMethodEnum deleteMessageMethodEnum)
        {
            var mess = sendedMessages.First(x => x.messageId == messageId);
            sendedMessages.Remove(mess);

            AddMessage(messageId, deleteMessageMethodEnum);
        }
        public void AddAction(ActionFrame actionFrame)
        {
            _inputActionsList.Add(actionFrame);
        }
        public void AddAction(List<ActionFrame> actionFrames)
        {
            _inputActionsList.AddRange(actionFrames);
        }
        internal void NextPage() => CompliteEvent.Invoke();
        internal void ValidationError(string Message) => ValidationErrorEvent.Invoke(Message);
        virtual internal void ChangeFlow(List<IPage> pages) => ChangePagesFlowByPagesPagesEvent.Invoke(pages);
        virtual internal void ChangeFlow(List<string> pages) => ChangePagesFlowPagesEvent.Invoke(pages);


        void PrepareBasicActions()
        {
            var passBtnAction = new ActionFrame
            {
                ActionObj = new ActionDelegate(NextPageAction),
                Trigger = () => PassBtnTrigger()
            };

            var pinMessageHendler = new ActionFrame
            {
                ActionObj = new ActionDelegate(PinMessageAction),
                Trigger = () => PinMessageTrigger()
            };

            AddAction(new List<ActionFrame>
            {
                passBtnAction,
                pinMessageHendler
            });
        }

        bool PassBtnTrigger() => Helpers.ValidatorHelpers.CallbackBtnActionValidate(CurrentUpdate, "pass");
        void NextPageAction(Update update) => NextPage();

        bool PinMessageTrigger()
        {
            if (CurrentUpdate.Message is null) return false;
            if (CurrentUpdate.Message.PinnedMessage is null) return false;
            return true;
        }
        void PinMessageAction(Update update) => AddMessage(CurrentUpdate.Message.MessageId, DeleteMessageMethodEnum.ClosePage);

    }

    public class ActionFrame
    {
        public delegate void ActionDelegate(Update update);
        public delegate bool ValidateDelegate(Update update);

        public required Func<bool> Trigger { get; init; }
        public ValidateDelegate? Validate { get; init; }
        public ActionDelegate ActionObj { get; init; }
    }
}
