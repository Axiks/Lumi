using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Pages.UpdateUser.Pages
{
    internal class UpdateSeccessUserPage(TelegramBotClient _botClient, UserContextModel _userContext, IUserService _userService, List<int> _sendMessages) : IPage
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
        public event ChangePagesFlowEventHandler? ChangePagesFlowPagesEvent;
        public event CompliteHandler? CompliteEvent;

        readonly string InitMessage = "Твої дані були успішно оновленні!";


        void IPage.SendInitMessage()
        {
            //ReloadUserContext();

            MessageSendHelper(InitMessage);
            CompliteEvent.Invoke();
        }

        void IPage.InputHendler(Update update)
        {

        }

        void MessageSendHelper(string text)
        {
            var mess = _botClient.SendMessage(_userContext.UpdateUser.TgId, text, parseMode: "HTML");

            //_sendMessages.Add(mess.MessageId);
        }

        async Task ReloadUserContext()
        {
            var user = await _userService.SignInUserAsync(_userContext.UpdateUser.TgId);
            //var user = await _userService.GetUser(_userContext.UpdateUser.TgId);
            _userContext.User = user;
        }
    }
}
