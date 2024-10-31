using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Pages.User
{
    internal class UserInfoPage : IPage
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
        readonly TelegramBotClient _botClient;
        readonly UserContextModel _userContext;
        readonly UserModel _userInfo;
        readonly List<int> _sendMessages;

        const string InitMessage = "<b>{0}</b>\n\n{1}\n\nЗамовлення: {2}\n\n<b>Доєднуйтесь до смоїх соцмереж:</b>\n\n{3}";

        public UserInfoPage(TelegramBotClient botClient, UserContextModel userContextModel, UserModel userInfo, List<int> sendMessages)
        {
            _botClient = botClient;
            _userContext = userContextModel;
            _userInfo = userInfo;
            _sendMessages = sendMessages;
        }

        void IPage.SendInitMessage()
        {
            string redyMessage = _userInfo.IsRadyForOrders ? "Reddy" : "No reddy";
            string links = _userInfo.Links is not null ? String.Join(", ", _userInfo.Links) : "No links";

            MessageSendHelper(string.Format(InitMessage, _userInfo.Nickname, _userInfo.About, redyMessage, links));
        }

        void IPage.InputHendler(Update update)
        {
            throw new NotImplementedException();
        }

        void MessageSendHelper(string text)
        {
            var mess = _botClient.SendMessage(_userContext.User.TelegramId, text, parseMode: "HTML");
            _sendMessages.Add(mess.MessageId);
        }
    }
}
