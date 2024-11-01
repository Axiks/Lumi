using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Entityes;
using Vanilla.TelegramBot.Helpers;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Pages.UpdateUser
{
    internal class UpdateIsRedyToWorkPage : IPage
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
        public event ChangePagesFlowEventHandler? ChangePagesFlowPagesEvent;
        public event CompliteHandler? CompliteEvent;

        readonly TelegramBotClient _botClient;
        readonly UserContextModel _userContext;
        readonly List<int> _sendMessages;
        BotUpdateUserModel _dataContext;

        readonly string InitMessage = "Чи ви приймаєте комерційні замовлення?";

        public UpdateIsRedyToWorkPage(TelegramBotClient botClient, UserContextModel userContext, List<int> sendMessages, BotUpdateUserModel dataContext)
        {
            _botClient = botClient;
            _userContext = userContext;
            _sendMessages = sendMessages;
            _dataContext = dataContext;
        }

        void IPage.SendInitMessage() {
            var pullOptions = GetPollOptions();
            var pollArgs = new SendPollArgs(_userContext.User.TelegramId, InitMessage, pullOptions);
            pollArgs.IsAnonymous = false;

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

            _dataContext.IsRadyForOrders = selectedOption == BoolPoolAnswerEnum.Yes ? true : false;
        }

        List<InputPollOption> GetPollOptions()
        {
            var pullOptions = new List<InputPollOption>
                {
                    new InputPollOption
                    {
                        Text = BoolPoolAnswerEnum.Yes.ToString(),
                    },
                    new InputPollOption
                    {
                        Text = BoolPoolAnswerEnum.No.ToString(),
                    },
                };

            return pullOptions;
        }

    }
}
