using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Pages.UpdateUser
{
    internal class UpdateUserAboutPage : IPage
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
        readonly TelegramBotClient _botClient;
        readonly UserContextModel _userContext;
        readonly List<int> _sendMessages;
        BotUpdateUserModel _dataContext;

        readonly string InitMessage = "Розкажіть більше про себе. Чим займаєтесь? Що полюбляєте?";

        public UpdateUserAboutPage(TelegramBotClient botClient, UserContextModel userContext, List<int> sendMessages, BotUpdateUserModel dataContext)
        {
            _botClient = botClient;
            _userContext = userContext;
            _sendMessages = sendMessages;
            _dataContext = dataContext;
        }

        void IPage.SendInitMessage() => MessageSendHelper(InitMessage);

        void IPage.InputHendler(Update update)
        {
            if (!ValidateInputType(update)) return;
            if (!ValidateInputData(update)) return;

            Action(update);
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

        void Action(Update update) => _dataContext.About = update.Message!.Text!;

        void MessageSendHelper(string text)
        {
            var mess = _botClient.SendMessage(_userContext.User.TelegramId, text);
            _sendMessages.Add(mess.MessageId);
        }

    }
}
