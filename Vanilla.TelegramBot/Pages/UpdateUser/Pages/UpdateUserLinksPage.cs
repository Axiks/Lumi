using System.ComponentModel.DataAnnotations;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.TelegramBot.Helpers;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.UI;

namespace Vanilla.TelegramBot.Pages.UpdateUser.Pages
{
    internal class UpdateUserLinksPage : IPage
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
        public event ChangePagesFlowEventHandler? ChangePagesFlowPagesEvent;
        public event CompliteHandler? CompliteEvent;

        readonly TelegramBotClient _botClient;
        readonly UserContextModel _userContext;
        readonly List<int> _sendMessages;

        readonly string InitMessage = "Залиш декілька посилань\n\nЦе можуть бути посилання на соц мережі, блоги, чи сторінки з твоїми проектами.\r\nМожна залишити декілька посилань, розділяючи їх через кому \",\"";

        public UpdateUserLinksPage(TelegramBotClient botClient, UserContextModel userContext, List<int> sendMessages)
        {
            _botClient = botClient;
            _userContext = userContext;
            _sendMessages = sendMessages;
        }

        void IPage.SendInitMessage() => MessageSendHelper(InitMessage, _userContext.User.Links);

        void IPage.InputHendler(Update update)
        {
            if (!ValidateInputType(update)) return;
            if (!ValidateInputData(update)) return;

            Action(update);

        }

        bool ValidateInputType(Update update)
        {
            if (update.Message is not null && update.Message.Text is not null)
            {
                _sendMessages.Add(update.Message.MessageId);
                return true;
            }
            else if (update.CallbackQuery is not null && update.CallbackQuery.Data is not null) return true;
            else ValidationErrorEvent.Invoke("Не те що очікувала. Введи текст!");


            return false;
        }

        bool ValidateInputData(Update update)
        {
            if (update.CallbackQuery is not null && update.CallbackQuery.Data is not null) return true;

            if (ValidatorHelpers.InlineBtnActionValidate(update, _userContext.ResourceManager.GetString("Pass"))) return true;

            if (update.Message!.Text!.Length > 4000)
            {
                ValidationErrorEvent.Invoke("Ойй ой ой\n\n Посилання, у сумні, не можуть бути таке довше за 4000 символи.");
                return false;
            }

            try
            {
                FormationHelper.Links(update.Message!.Text!, _userContext);
            }
            catch (ValidationException e)
            {
                ValidationErrorEvent.Invoke("Ойй ой ой\n\n Я на розпізнанала усі посилання \n Далі інфа про те як його правильно писати");
                return false;
            }
            catch (Exception e)
            {
                throw e;
            }

            return true;
        }

        void Action(Update update)
        {
            if (ValidatorHelpers.CallbackBtnActionValidate(update, "pass"))
            {
                CompliteEvent.Invoke();
                return;
            }
            else if (ValidatorHelpers.InlineBtnActionValidate(update, _userContext.ResourceManager.GetString("Pass")))
            {
                CompliteEvent.Invoke();
                return;
            }
            else if (update.Message is not null && update.Message.Text is not null)
            {
                _userContext.User.Links = new List<string>(FormationHelper.Links(update.Message!.Text!, _userContext));
                CompliteEvent.Invoke();
            }
            else ValidationErrorEvent.Invoke("action not recognized");
        }

        void MessageSendHelper(string text, List<string>? links = null)
        {
            string? myLinksStr = null;

            if (links is not null && links.Count() > 0)
            {
                myLinksStr = links[0];

                for (int i = 1; i < links.Count(); i++) myLinksStr += links[i];
            }

            var mess = _botClient.SendMessage(_userContext.User.TelegramId, text, replyMarkup: Keyboards.GetPassKeypoard(_userContext), parseMode: "HTML");
            _sendMessages.Add(mess.MessageId);
        }

    }
}
