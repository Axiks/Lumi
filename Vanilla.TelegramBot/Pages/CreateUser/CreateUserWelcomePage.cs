using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.TelegramBot.Helpers;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.UI;

namespace Vanilla.TelegramBot.Pages.CreateUser
{
    internal class CreateUserWelcomePage : IPage
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
        public event ChangePagesFlowEventHandler? ChangePagesFlowPagesEvent;
        public event CompliteHandler? CompliteEvent;

        readonly TelegramBotClient _botClient;
        readonly UserContextModel _userContext;
        List<SendedMessageModel> _sendedMessages;

        readonly string InitMessage = "Перед тим як почати використовувати бота, вам необхідно створити профіль!";

        public CreateUserWelcomePage(TelegramBotClient botClient, UserContextModel userContext, List<SendedMessageModel> sendedMessages)
        {
            _botClient = botClient;
            _userContext = userContext;
            _sendedMessages = sendedMessages;
        }

        void IPage.SendInitMessage()
        {
            MessageSendHelper(InitMessage);
            //CompliteEvent.Invoke();
        }

        void IPage.InputHendler(Update update)
        {
            if (!ValidateInputType(update)) return;
            if (!ValidateInputData(update)) return;

            Action(update);
        }

        bool ValidateInputType(Update update)
        {
            if (update.CallbackQuery is not null && update.CallbackQuery.Data is not null) return true;
            else
            {
                _sendedMessages.Add(new SendedMessageModel(update.Message.MessageId, Common.Enums.DeleteMessageMethodEnum.NextMessage));
                ValidationErrorEvent.Invoke("Не те що очікувала. Обери дію з кнопки");
            }

            return false;
        }

        bool ValidateInputData(Update update) => ValidatorHelpers.CallbackBtnActionValidate(update, "CreateProfile");

        void Action(Update update)
        {
            if (ValidatorHelpers.CallbackBtnActionValidate(update, "CreateProfile"))
            {
                CompliteEvent.Invoke();
                return;
            }
            else ValidationErrorEvent.Invoke("action not recognized");
        }

        void MessageSendHelper(string text)
        {
            var mess = _botClient.SendMessage(_userContext.UpdateUser.TgId, text, replyMarkup: Keyboards.GetCreateProfileKeypoard(_userContext.ResourceManager), parseMode: "HTML");
            _sendedMessages.Add(new SendedMessageModel(mess.MessageId, Common.Enums.DeleteMessageMethodEnum.ClosePage));
        }
    }
}
