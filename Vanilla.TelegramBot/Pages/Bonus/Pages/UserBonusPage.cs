﻿using Telegram.BotAPI;
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
    internal class UserBonusPage : IPage, IPageChangeFlowExtension, IPageKeyboardExtension
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
        public event ChangePagesFlowEventHandler? ChangePagesFlowPagesEvent;
        public event CompliteHandler? CompliteEvent;
        public event ChangePagesFlowByPagesEventHandler? ChangePagesFlowByPagesPagesEvent;
        public event ChangeInlineKeyboardHandler? ChangeInlineKeyboardEvent;

        readonly TelegramBotClient _botClient;
        readonly UserContextModel _userContext;
        readonly List<int> _sendMessages;
        readonly IBonusService _bonusService;

        string _deliver = " mya~ ";

        int _initMessageId;

        readonly string InitMessage = "Тут ти можеш переглядати, та активовувати бонуси, отримані від інших творців.\n\n<i>На даний момент співпацюємо виключно з <a href='https://t.me/pro_vision_ua'>PRO.Vision</a></i>";
        List<int> _pageSendMessages;
        bool _isChangeBonus;

        public bool BackBtnIntercept => false;

        List<SendedMessageModel> _sendedMessages;

        public UserBonusPage(TelegramBotClient botClient, UserContextModel userContext, List<int> sendMessages, IBonusService bonusService, List<SendedMessageModel> sendedMessages)
        {
            _botClient = botClient;
            _userContext = userContext;
            _sendMessages = sendMessages;
            _bonusService = bonusService;

            _pageSendMessages = new List<int>();
            _isChangeBonus = false;

            _sendedMessages = sendedMessages;
        }

        void IPage.SendInitMessage() => InitMessageSendHelper(InitMessage);
        void IPage.InputHendler(Update update)
        {
            if (!ValidateInputType(update)) return;
            if (!ValidateInputData(update)) return;

            Action(update);
        }


        void InitMessageSendHelper(string text)
        {


      /*      if(_catalogInitMessageId is null)
            {
                var enterMess = _botClient.SendMessage(_userContext.User.TelegramId, "Bonus system", replyMarkup: Keyboards.BackKeyboard(_userContext), parseMode: "HTML");
                //_sendMessages.Add(enterMess.MessageId);
                _catalogInitMessageId = enterMess.MessageId;
            }
            else
            {
                _botClient.EditMessageReplyMarkup(chatId: _userContext.User.TelegramId, messageId: _catalogInitMessageId ?? 0, replyMarkup: Keyboards.BackKeyboard(_userContext));
            }*/

            //ChangeInlineKeyboardEvent.Invoke(Keyboards.BackKeyboard(_userContext));

            if (_isChangeBonus is false)
            {
                //var mess = _botClient.SendMessage(_userContext.User.TelegramId, text, replyMarkup: Keyboards.CannelKeyboard(_userContext, placeholder: _userContext.User.Nickname), parseMode: "HTML");
                var mess = _botClient.SendMessage(_userContext.User.TelegramId, text, replyMarkup: GenerateBonusKeyboard(GetActivatedBonuses(), GetUnactivatedBonuses()), parseMode: "HTML");
                _initMessageId = mess.MessageId;

                _sendedMessages.Add(new SendedMessageModel(mess.MessageId, DeleteMessageMethodEnum.ExitFolder));
                //_sendMessages.Add(mess.MessageId);
                _pageSendMessages.Add(mess.MessageId);
            }
            else
            {
                var mess = _botClient.EditMessageReplyMarkup(chatId: _userContext.User.TelegramId, messageId: _initMessageId, replyMarkup: GenerateBonusKeyboard(GetActivatedBonuses(), GetUnactivatedBonuses(), isWithAllBonuses: true));
            }
        }

        bool ValidateInputType(Update update)
        {
            if (update.CallbackQuery is not null && update.CallbackQuery.Data is not null) return true;
            else ValidationErrorEvent.Invoke("Не те що очікувала. Обери дію з кнопки!");

            return false;
        }

        bool ValidateInputData(Update update)
        {
            if (ValidatorHelpers.CallbackBtnActionValidate(update, "view activated bonuses")) return true;
            else if (update.CallbackQuery.Data.Contains("bonus" + _deliver)) return true;

            return  false;
        }

        void Action(Update update)
        {
            var command = update.CallbackQuery.Data;

            if (ValidatorHelpers.CallbackBtnActionValidate(update, "view activated bonuses"))
            {
                // Update keyboard
                _botClient.EditMessageReplyMarkup(chatId: _userContext.User.TelegramId, messageId: _initMessageId, replyMarkup: GenerateBonusKeyboard(GetActivatedBonuses(), GetUnactivatedBonuses(), isWithAllBonuses: true));

                return;
            }
            else if (command.Contains("bonus" + _deliver))
            {
                // Open card with bonus
                int bonusId = Convert.ToInt32(command.Split(_deliver).Last());
                OpenBonus(bonusId);
            }
            else throw new Exception("command not recognized");
        }

        void OpenBonus(long bonusId)
        {
            _isChangeBonus = true;
            var bonusObj = new UserBonusInfoPage(bonusId, _botClient, _userContext, _bonusService, _sendMessages, _sendedMessages);
            List<IPage> pages = new List<IPage> {
                bonusObj
            };
            ChangePagesFlowByPagesPagesEvent.Invoke(pages);
        }

        List<UserBonusModel> GetUserBonuses() => _bonusService.GetUserBonuses(_userContext.User.TelegramId);
        List<UserBonusModel> GetActivatedBonuses() => GetUserBonuses().Where(x => x.IsUsed is false).ToList();
        List<UserBonusModel> GetUnactivatedBonuses() => GetUserBonuses().Where(x => x.IsUsed is true).ToList();

        InlineKeyboardMarkup GenerateBonusKeyboard(List<UserBonusModel> unActivatedBonuses, List<UserBonusModel>? activatedBonuses = null, bool isWithAllBonuses = false)
        {
            List<List<InlineKeyboardButton>> rowWithKeysBonus = new List<List<InlineKeyboardButton>>();
            List<InlineKeyboardButton> keysWithBonus = new List<InlineKeyboardButton>();

            var bonusList = new List<UserBonusModel>(unActivatedBonuses);

            if (isWithAllBonuses == true && activatedBonuses is not null) bonusList.AddRange(activatedBonuses);

            // Keyboard generator
            foreach (var bonus in bonusList)
            {
                var btnText = bonus.ShortTitle ?? "not data";
                if (bonus.IsUsed) btnText = "✔️ " + btnText;
                var btn = new InlineKeyboardButton(text: btnText);
                btn.CallbackData = "bonus" + _deliver + bonus.BonusId.ToString();

                if (keysWithBonus.Count >= 2)
                {
                    rowWithKeysBonus.Add(keysWithBonus);
                    keysWithBonus = new List<InlineKeyboardButton>();
                }
                keysWithBonus.Add(btn);
            }

            if (keysWithBonus is not null) rowWithKeysBonus.Add(keysWithBonus);

            if(isWithAllBonuses == false && activatedBonuses is not null && activatedBonuses.Count() > 0)
            {
                keysWithBonus = new List<InlineKeyboardButton>();
                var usedBonusesBtn = new InlineKeyboardButton(text: string.Format(_userContext.ResourceManager.GetString("ActivatedBonusesLabel"), activatedBonuses.Count()));
                usedBonusesBtn.CallbackData = "view activated bonuses";
                // Used Bonuses
                keysWithBonus.Add(usedBonusesBtn);
                rowWithKeysBonus.Add(keysWithBonus);
            }

            return new InlineKeyboardMarkup
            (
                rowWithKeysBonus.ToArray()
            );
        }
    }
}
