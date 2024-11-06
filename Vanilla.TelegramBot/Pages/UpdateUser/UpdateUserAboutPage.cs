using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.UI;

namespace Vanilla.TelegramBot.Pages.UpdateUser
{
    internal class UpdateUserAboutPage : IPage
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
        public event ChangePagesFlowEventHandler? ChangePagesFlowPagesEvent;
        public event CompliteHandler? CompliteEvent;

        readonly TelegramBotClient _botClient;
        readonly UserContextModel _userContext;
        readonly List<int> _sendMessages;

        //readonly string InitMessage = "Розкажіть більше про себе. Чим займаєтесь? Що полюбляєте?";
        readonly string InitMessage = "Розкажіть про себе\n\n<i>Це можуть бути ваші захоплення, цінності, навички, мрії, цілі так і будь яка інша інформація про себе, про яку ви хотіли би розказати</i>";

        public UpdateUserAboutPage(TelegramBotClient botClient, UserContextModel userContext, List<int> sendMessages)
        {
            _botClient = botClient;
            _userContext = userContext;
            _sendMessages = sendMessages;
        }

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
            if (update.Message!.Text!.Length >= 4000)
            {
                ValidationErrorEvent.Invoke("Ойй ой ой\n\n Опис не може бути таке довше за 4000 символи.");
                return false;
            }
            return true;
        }

        void Action(Update update) => _userContext.User.About = update.Message!.Text!;

        void MessageSendHelper(string text)
        {
            //var mess = _botClient.SendMessage(_userContext.User.TelegramId, text, replyMarkup: Keyboards.CannelKeyboard(_userContext, placeholder: _userContext.User.About), parseMode: "HTML");
            var mess = _botClient.SendMessage(_userContext.User.TelegramId, text, parseMode: "HTML");
            _sendMessages.Add(mess.MessageId);
        }

    }
}
