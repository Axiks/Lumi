using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages.UpdateUser.Models;

namespace Vanilla.TelegramBot.Pages.UpdateUser.Pages
{
    internal class UpdateUserNicknamePage(TelegramBotClient _botClient, UserContextModel _userContext, List<int> _sendMessages, UserActionContextModel _updateUserActionContextModel) : IPage
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
        public event ChangePagesFlowEventHandler? ChangePagesFlowPagesEvent;
        public event CompliteHandler? CompliteEvent;

        readonly string InitMessage = "Як вас звати?\n<i>Це може бути як реальне ім'я, так і нікнейм</i>";

        void IPage.SendInitMessage() => MessageSendHelper(InitMessage);

        void IPage.InputHendler(Update update)
        {
            if (!ValidateInputType(update)) return;
            if (!ValidateInputData(update)) return;

            _sendMessages.Add(update.Message.MessageId);

            Action(update);

            CompliteEvent.Invoke();
        }
        bool ValidateInputType(Update update)
        {
            if (update.Message is null || update.Message.Text is null)
            {
                ValidationErrorEvent.Invoke("Не те що очікувала. Введи текст!");
                return false;
            }
            return true;
        }

        bool ValidateInputData(Update update)
        {
            if (update.Message!.Text!.Length >= 64)
            {
                ValidationErrorEvent.Invoke("Ойй ой ой\n\n Нікнейм не може бути таке довше за 64 символи.");
                return false;
            }
            return true;
        }

        void Action(Update update) => _updateUserActionContextModel.Nickname = update.Message!.Text!;

        void MessageSendHelper(string text)
        {
            //var mess = _botClient.SendMessage(_userContext.UpdateUser.TelegramId, text, replyMarkup: Keyboards.CannelKeyboard(_userContext, placeholder: _userContext.UpdateUser.Nickname), parseMode: "HTML");
            var mess = _botClient.SendMessage(_userContext.UpdateUser.TgId, text, parseMode: "HTML");

            _sendMessages.Add(mess.MessageId);
        }

    }
}
