using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Helpers;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla_App.Interfaces;
using Vanilla_App.Models;

namespace Vanilla.TelegramBot.Pages.Bonus.Pages
{
    public class UserBonusInfoPage : IPage
    {
        long _bonus_id;
        TelegramBotClient _botClient;
        UserContextModel _userContext;
        IBonusService _bonusService;
        List<int> _sendMessages;

        public event ValidationErrorEventHandler? ValidationErrorEvent;
        public event ChangePagesFlowEventHandler? ChangePagesFlowPagesEvent;
        public event CompliteHandler? CompliteEvent;

        private string _deliver = " mya~ ";

        private string _bonusUrl = "https://cdn4.cdn-telegram.org/file/sdfey5DeW7gixp3VffGrQpRg5ru84-vOjFCtwOQKKIX2gDKj0Am5K9IkoYRtcvMz2NYociqLh8VUbTyZomHYocX1ZKo0X_mXtCvHlGL0NSuWQmQvVgNhXrMFpEAYhGBftvYI9ceBHn8kH49ozWeGR9kXQt-FDEkbPgBHJsRqlP0Edyp_ZbrE4HkWAA3ctcUmO6moF3kqLli45OLY2BouOhVRjkcxhflv2jdqKacjC7iHvP6lKl8iDXzCrezF9VtmU1BiUvG02f0jSnTsFPxRK5eI1L06Xzlhy7kfNAqkyfx4366DGWtrUE6iOzXHvijRGBS8qDQrxmeJ6XuSUag0UA.jpg";

        UserBonusModel _bonusObject;

        List<SendedMessageModel> _sendedMessages;
        public UserBonusInfoPage(long bonus_id, TelegramBotClient botClient, UserContextModel userContext, IBonusService bonusService, List<int> sendMessages, List<SendedMessageModel> sendedMessages)
        {
            _bonus_id = bonus_id;
            _botClient = botClient;
            _userContext = userContext;
            _bonusService = bonusService;
            _sendMessages = sendMessages;

            _sendedMessages = sendedMessages;

            if (IsUserBonus(bonus_id) is false) throw new Exception("is`nt user bonus");
            _bonusObject = GetUserBonus(bonus_id, bonusService);
        }

        void IPage.SendInitMessage() => SendBonusInfo();
        void IPage.InputHendler(Update update)
        {
            if (!ValidateInputType(update)) return;
            if (!ValidateInputData(update)) return;

            Action(update);
        }

        bool ValidateInputType(Update update)
        {
            if (update.CallbackQuery is not null && update.CallbackQuery.Data is not null) return true;
            else ValidationErrorEvent.Invoke("Не те що очікувала. Обери дію з кнопки!");

            return false;
        }

        bool ValidateInputData(Update update)
        {
            if (ValidatorHelpers.CallbackBtnActionValidate(update, "ok")) return true;

            return false;
        }

        void Action(Update update)
        {
            if (ValidatorHelpers.CallbackBtnActionValidate(update, "ok")) ActionOk(update);

        }

        void ActionOk(Update update)
        {
            if (_bonusObject.IsUsed is true) throw new Exception("Bonus is be used");

            _bonusService.TakeBonus(_bonusObject.BonusId);
            var message = string.Format("Bonus {0} has been successfully spent!", _bonusObject.Title);

            _botClient.EditMessageText(chatId: _userContext.User.TelegramId, messageId: update.CallbackQuery.Message.MessageId, text: message, parseMode: "HTML");

            CompliteEvent.Invoke();
        }

        void SendBonusInfo()
        {
            if (_bonusObject.IsUsed is true)
            {
                //SendBonusInfoMessage(isDelete: false);
                SendBonusInfoMessage(DeleteMessageMethodEnum.ClosePage);
                //CompliteEvent.Invoke();
            }
            else
            {
                /*string message = string.Format("{0} \n\n{1}\n\nЗареєстровано: {2}", _bonusObject.Title, _bonusObject.Description, _bonusObject.DateOfRegistration.ToString("dd.MM.yyyy"));
                SendBonusActionMessage(message, GenerateBonusInfoKeyboard());*/
                SendBonusInfoMessage(DeleteMessageMethodEnum.ClosePage);
                SendBonusInfoActions();
            }
        }

        UserBonusModel GetUserBonus(long bonusId, IBonusService bonusService)
        {
            var bonus = bonusService.GetBonus(_bonus_id);
            if (bonus is null) throw new Exception("this id not found");
            //if (IsUserBonus(bonusId) is false) throw new Exception("is`nt user bonus");

            return (UserBonusModel)bonus;
        }

        bool IsUserBonus(long bonusId) => _bonusService.GetUserBonuses(_userContext.User.TelegramId).Any(x => x.BonusId == bonusId);

 /*       void SendBonusActionMessage(string message, InlineKeyboardMarkup? keyboard = null, bool isDelete = true)
        {
            *//* var bonusMessage = _botClient.SendPhoto(chatId: _userContext.User.TelegramId, caption: message, photo: _bonusUrl);
             //if(isDelete) _sendMessages.Add(bonusMessage.MessageId);

             string activateDate = _bonusObject.DateOfUsed?.ToString("dd.MM.yyyy");

             if (_bonusObject.IsUsed)
             {
                 message += string.Format("Бонус було успішно активовано: {0}", activateDate);
             }*/

            /*if(keyboard is not null)
            {
                SendMessageArgs mes = new SendMessageArgs(_userContext.User.TelegramId, "Чи хочеш використати бонус?") { ParseMode = "HTML" };
                mes.ReplyMarkup = keyboard;

                var messageObj = _botClient.SendMessage(mes);
                //_sendedMessages.Add(new SendedMessageModel(messageObj.MessageId, DeleteMessageMethodEnum.ExitFolder));
                //_sendMessages.Add(messageObj.MessageId);
            }*//*

            SendBonusInfoMessage();
            if (keyboard is not null) SendMessageActions();
        }*/

        void SendBonusInfoMessage(DeleteMessageMethodEnum deleteMessageMethodEnum)
        {
            string message = string.Format("{0} \n\n{1}\n\nЗареєстровано: {2}", _bonusObject.Title, _bonusObject.Description, _bonusObject.DateOfRegistration.ToString("dd.MM.yyyy"));
            if (_bonusObject.IsUsed)
            {
                string activateDate = _bonusObject.DateOfUsed?.ToString("dd.MM.yyyy");
                message += string.Format("\nБонус було успішно активовано: {0}", activateDate);
            }

            var messageObj = _botClient.SendPhoto(chatId: _userContext.User.TelegramId, caption: message, photo: _bonusUrl, parseMode: "HTML");
            _sendedMessages.Add(new SendedMessageModel(messageObj.MessageId, deleteMessageMethodEnum));
        }

        void SendBonusInfoActions()
        {
            SendMessageArgs mes = new SendMessageArgs(_userContext.User.TelegramId, "Чи хочеш використати бонус?") { ParseMode = "HTML" };
            mes.ReplyMarkup = GenerateBonusInfoKeyboard();

            var messageObj = _botClient.SendMessage(mes);
            _sendedMessages.Add(new SendedMessageModel(messageObj.MessageId, DeleteMessageMethodEnum.ClosePage));
        }

        /*     void SendBonusInfoMessage(bool isDelete = true)
             {
                 string text = string.Format("{0} \n\n{1}\n\nЗареєстровано: {2}\n\nБонус було успішно активовано: {3}", _bonusObject.Title, _bonusObject.Description, _bonusObject.DateOfRegistration.ToString("dd.MM.yyyy"), _bonusObject.DateOfUsed?.ToString("dd.MM.yyyy"));
                 var messageObj = _botClient.SendPhoto(chatId: _userContext.User.TelegramId, caption: text, photo: _bonusUrl, parseMode: "HTML");
                 //if(isDelete) _sendMessages.Add(messageObj.MessageId);
                 _sendedMessages.Add(new SendedMessageModel(messageObj.MessageId, DeleteMessageMethodEnum.NextMessage));
             }*/

        private InlineKeyboardMarkup GenerateBonusInfoKeyboard()
        {
            var yesBtn = new InlineKeyboardButton(text: _userContext.ResourceManager.GetString("Spend"));
            //var noBtn = new InlineKeyboardButton(text: _userContext.ResourceManager.GetString("No"));

            var replyMarkuppp = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                        new InlineKeyboardButton[]{
                            yesBtn,
                            //noBtn
                         }
                }
            );

            yesBtn.CallbackData = "ok";
            //noBtn.CallbackData = userContext.ResourceManager.GetString("Cannel"); // don`t work

            return replyMarkuppp;
        }

    }
}
