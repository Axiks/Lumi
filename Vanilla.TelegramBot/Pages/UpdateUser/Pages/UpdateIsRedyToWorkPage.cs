using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages.UpdateUser.Models;

namespace Vanilla.TelegramBot.Pages.UpdateUser.Pages
{
    internal class UpdateIsRedyToWorkPage(TelegramBotClient _botClient, UserContextModel _userContext, List<int> _sendMessages, UserActionContextModel _updateUserActionContextModel) : IPage
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
        public event ChangePagesFlowEventHandler? ChangePagesFlowPagesEvent;
        public event CompliteHandler? CompliteEvent;

        readonly string InitMessage = "Чи ви приймаєте комерційні замовлення?";


        void IPage.SendInitMessage()
        {
            var pullOptions = GetPollOptions();
            var pollArgs = new SendPollArgs(_userContext.UpdateUser.TgId, InitMessage, pullOptions);
            pollArgs.IsAnonymous = false;
            //pollArgs.ReplyMarkup = Keyboards.CannelKeyboard(_userContext);

            var sendedMessage = _botClient.SendPoll(pollArgs);

            _sendMessages.Add(sendedMessage.MessageId);
        }

        void IPage.InputHendler(Update update)
        {
            if (!ValidateInputType(update)) return;
            if (!ValidateInputData(update)) return;

            Action(update);

            CompliteEvent.Invoke();
        }

        bool ValidateInputType(Update update)
        {
            if (update.PollAnswer is null || update.PollAnswer.OptionIds is null)
            {
                ValidationErrorEvent.Invoke("Не те що очікувала. Дай відповідь на опитування");
                return false;
            }
            return true;
        }

        bool ValidateInputData(Update update) => true;

        void Action(Update update)
        {
            var optionIndex = update.PollAnswer.OptionIds.First();

            var boolPoolAnswer = Enum.GetValues(typeof(BoolPoolAnswerEnum)).Cast<BoolPoolAnswerEnum>().ToList();

            var selectedOption = boolPoolAnswer[optionIndex];

            _updateUserActionContextModel.IsRadyForOrders = selectedOption == BoolPoolAnswerEnum.Yes ? true : false;
        }

        List<InputPollOption> GetPollOptions()
        {
            var pullOptions = new List<InputPollOption>
                {
                    new InputPollOption(BoolPoolAnswerEnum.Yes.ToString()),
                    new InputPollOption(BoolPoolAnswerEnum.No.ToString()),
                };

            return pullOptions;
        }

    }
}
