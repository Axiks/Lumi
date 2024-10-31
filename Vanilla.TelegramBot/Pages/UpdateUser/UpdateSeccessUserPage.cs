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
        readonly TelegramBotClient _botClient;
        readonly UserContextModel _userContext;
        readonly List<int> _sendMessages;
        BotUpdateUserModel _dataContext;

        readonly string InitMessage = "Твої дані були успішно оновленні!";

        public UpdateSeccessUserPage(TelegramBotClient botClient, UserContextModel userContext, List<int> sendMessages, BotUpdateUserModel dataContext)
        {
            _botClient = botClient;
            _userContext = userContext;
            _sendMessages = sendMessages;
            _dataContext = dataContext;
        }

        void IPage.SendInitMessage() => MessageSendHelper(InitMessage);

        void IPage.InputHendler(Update update)
        {

        }

        void MessageSendHelper(string text)
        {
            var mess = _botClient.SendMessage(_userContext.User.TelegramId, text);
            _sendMessages.Add(mess.MessageId);
        }
    }
}
