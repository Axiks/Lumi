using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Helpers;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.UI.Widgets;
using Vanilla_App.Services.Bonus;

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

        readonly string InitMessage = "Тут ти можеш переглядати, та активовувати бонуси, отримані від інших творців.\n\n<i>На даний момент співпацюємо виключно з <a href='https://t.me/pro_vision_ua'>PRO.Vision</a></i>\nБонуси нараховуються згідно програми лояльності креативної майстерні PRO/LAB";
        List<int> _pageSendMessages;
        bool _isChangeBonus;

        public bool BackBtnIntercept => false;

        List<SendedMessageModel> _sendedMessages;

        //bool _isServerOnline = false;

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

        void IPage.SendInitMessage() {
            // Server offline fix
            try
            {
                //_bonusService.GetUserBonuses(_userContext.User.TelegramId);
                if (_bonusService.IsOnline() is false) ServerOffline();
            }
            catch (HttpRequestException error)
            {
                ServerOffline();
            }

            void ServerOffline()
            {
                ProblemWithGetDataFromServerMessage();
                CompliteEvent.Invoke();
                return;
            }

            InitMessageSendHelper(InitMessage);
        }
        void IPage.InputHendler(Update update)
        {
            if (!ValidateInputType(update)) return;
            if (!ValidateInputData(update)) return;

            Action(update);
        }


        void InitMessageSendHelper(string text)
        {
            if (_isChangeBonus is false)
            {
                //var mess = _botClient.SendMessage(_userContext.User.TelegramId, text, replyMarkup: Keyboards.CannelKeyboard(_userContext, placeholder: _userContext.User.Nickname), parseMode: "HTML");
                SendMessageArgs messageArgs = new SendMessageArgs(_userContext.User.TelegramId, text) { 
                    ParseMode = "HTML"
                };
                if (IsUserHaveBonuses() is true) messageArgs.ReplyMarkup = GenerateBonusKeyboard(GetActivatedBonuses(), GetUnactivatedBonuses());

                var mess = _botClient.SendMessage(messageArgs);

                _initMessageId = mess.MessageId;

                _sendedMessages.Add(new SendedMessageModel(mess.MessageId, DeleteMessageMethodEnum.ExitFolder));
                //_sendMessages.Add(mess.MessageId);
                _pageSendMessages.Add(mess.MessageId);
            }
            else
            {
                SendMessageArgs messageArgs = new SendMessageArgs(_userContext.User.TelegramId, text)
                {
                    ParseMode = "HTML"
                };
                if (IsUserHaveBonuses() is true) messageArgs.ReplyMarkup = GenerateBonusKeyboard(GetActivatedBonuses(), GetUnactivatedBonuses(), isWithAllBonuses: true);
                var mess = _botClient.SendMessage(messageArgs);
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
            else if (command.Contains("bonus" + _deliver) && command.Contains("ok"))
            {
                string bonusId = command.Split(_deliver)[1];

                _bonusService.TakeBonus(bonusId);

                var bonus = _bonusService.GetBonus(bonusId);

                var message = string.Format("Bonus {0} has been successfully spent!", bonus.Title);
                //_botClient.EditMessageText(chatId: _userContext.User.TelegramId, messageId: update.CallbackQuery.Message.MessageId, text: message, parseMode: "HTML");


                var media = new InputMediaPhoto(bonus.CoverUrl);
                media.Caption = Widjets.BonusInfo(_userContext.User.TelegramId, bonus).Caption; // fix
                media.Caption = media.Caption + "\n\nБонус успішно активовано!";

                _botClient.EditMessageMedia(chatId: _userContext.User.TelegramId, messageId: update.CallbackQuery.Message.MessageId, media: media);

                var mess = _botClient.EditMessageReplyMarkup(chatId: _userContext.User.TelegramId, messageId: _initMessageId, replyMarkup: GenerateBonusKeyboard(GetActivatedBonuses(), GetUnactivatedBonuses(), isWithAllBonuses: true));
            }
            else if (command.Contains("bonus" + _deliver))
            {
                // Open card with bonus
                string bonusId = command.Split(_deliver).Last();
                OpenBonus(bonusId);
            }
            else throw new Exception("command not recognized");
        }


        void OpenBonus(string bonusId)
        {
            _isChangeBonus = true;
      /*      var bonusObj = new UserBonusInfoPage(bonusId, _botClient, _userContext, _bonusService, _sendMessages, _sendedMessages);
            List<IPage> pages = new List<IPage> {
                bonusObj
            };*/
            //ChangePagesFlowByPagesPagesEvent.Invoke(pages);

            SendInformationAboutBonus(bonusId);
        }

        void SendInformationAboutBonus(string bonusId)
        {
            var bonus = _bonusService.GetBonus(bonusId);
            var arg = Widjets.BonusInfo(_userContext.User.TelegramId, bonus, _userContext);
            var messageObj = _botClient.SendPhoto(arg);
            _sendedMessages.Add(new SendedMessageModel(messageObj.MessageId, DeleteMessageMethodEnum.ExitFolder));
        }

        List<UserBonusModel>? GetUserBonuses()
        {
            return _bonusService.GetUserBonuses(_userContext.User.TelegramId);
        }
        void ProblemWithGetDataFromServerMessage()
        {
            SendMessageArgs mes = new SendMessageArgs(_userContext.User.TelegramId, Widjets.ProblemWithExternalServer()) { ParseMode = "HTML" };
            var messageObj = _botClient.SendMessage(mes);
            _sendedMessages.Add(new SendedMessageModel(messageObj.MessageId, DeleteMessageMethodEnum.None));
        }
        bool IsUserHaveBonuses() {
            var bonuses = GetUserBonuses();
            if (bonuses.IsNullOrEmpty()) return false;
            return true;
        }
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
