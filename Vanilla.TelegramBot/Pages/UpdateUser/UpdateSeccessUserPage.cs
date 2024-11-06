using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Pages.UpdateUser
{
    internal class UpdateSeccessUserPage : IPage
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
        public event ChangePagesFlowEventHandler? ChangePagesFlowPagesEvent;
        public event CompliteHandler? CompliteEvent;

        readonly TelegramBotClient _botClient;
        readonly UserContextModel _userContext;
        readonly List<int> _sendMessages;

        readonly string InitMessage = "Твої дані були успішно оновленні!";

        public UpdateSeccessUserPage(TelegramBotClient botClient, UserContextModel userContext, List<int> sendMessages)
        {
            _botClient = botClient;
            _userContext = userContext;
            _sendMessages = sendMessages;
        }

        void IPage.SendInitMessage(){
            MessageSendHelper(InitMessage);
            CompliteEvent.Invoke();
        }

        void IPage.InputHendler(Update update)
        {

        }

        void MessageSendHelper(string text)
        {
            var mess = _botClient.SendMessage(_userContext.User.TelegramId, text, parseMode: "HTML");

            //_sendMessages.Add(mess.MessageId);
        }
    }
}
