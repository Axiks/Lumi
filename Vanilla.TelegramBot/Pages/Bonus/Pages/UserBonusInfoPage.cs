using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Abstract;
using Vanilla.TelegramBot.Helpers;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.UI.Widgets;
using Vanilla_App.Interfaces;
using Vanilla_App.Models;
using static Vanilla.TelegramBot.Abstract.ActionFrame;

namespace Vanilla.TelegramBot.Pages.Bonus.Pages
{
    public class UserBonusInfoPage : BasicPageAbstract, IPage
    {
        string _bonus_id;
        TelegramBotClient _botClient;
        UserContextModel _userContext;
        IBonusService _bonusService;
        List<int> _sendMessages;

        public event ValidationErrorEventHandler? ValidationErrorEvent;
        public event ChangePagesFlowEventHandler? ChangePagesFlowPagesEvent;
        public event CompliteHandler? CompliteEvent;

        private string _deliver = " mya~ ";

        UserBonusModel? _bonusObject;

        List<SendedMessageModel> _sendedMessages;

        bool _isServerOnline = true;

        public UserBonusInfoPage(string bonus_id, TelegramBotClient botClient, UserContextModel userContext, IBonusService bonusService, List<int> sendMessages, List<SendedMessageModel> sendedMessages) : base(botClient, userContext, sendedMessages)
        {
            _bonus_id = bonus_id;
            _botClient = botClient;
            _userContext = userContext;
            _bonusService = bonusService;
            _sendMessages = sendMessages;

            _sendedMessages = sendedMessages;

            if (IsUserBonus(bonus_id) is false) throw new Exception("is`nt user bonus");

            try
            {
                _bonusObject = GetUserBonus(bonus_id, bonusService);
            }
            catch(HttpRequestException error)
            {
                // Problem connect to server
                _isServerOnline = false;
            }
        }

        /*        void IPage.SendInitMessage() => SendBonusInfo();
                void IPage.InputHendler(Update update)
                {
                    if (!ValidateInputType(update)) return;
                    if (!ValidateInputData(update)) return;

                    Action(update);
                }*/


        public override void InitMessage()
        {
            if (_isServerOnline == false) {
                ProblemWithGetDataFromServerMessage();
                CompliteEvent.Invoke();
                return;
            }

            SendBonusInfo();
        }

        public override void InitActions()
        {
            var okBtnAction = new ActionFrame
            {
                ActionObj = new ActionDelegate(ActionOk),
                Trigger = () => ConfirmOkBtnInputTrigger(),
            };

            AddAction(new List<ActionFrame> {
                okBtnAction
            });
        }

        bool ConfirmOkBtnInputTrigger() => Helpers.ValidatorHelpers.CallbackBtnActionValidate(CurrentUpdate, "ok");

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
                SendBonusInfoMessage(DeleteMessageMethodEnum.ClosePage);
            }
            else
            {
                SendBonusInfoMessage(DeleteMessageMethodEnum.ClosePage);
                SendBonusInfoActions();
            }
        }

        UserBonusModel? GetUserBonus(string bonusId, IBonusService bonusService)
        {
            var bonus = bonusService.GetBonus(_bonus_id);
            if (bonus is null) throw new Exception("this id not found");
            //if (IsUserBonus(bonusId) is false) throw new Exception("is`nt user bonus");

            return bonus;
            //return (UserBonusModel)bonus;
        }

        bool IsUserBonus(string bonusId) => _bonusService.GetUserBonuses(_userContext.User.TelegramId).Any(x => x.BonusId == bonusId);


        void SendBonusInfoMessage(DeleteMessageMethodEnum deleteMessageMethodEnum)
        {
            string message = string.Format("{0} \n\n{1}\n\nЗареєстровано: {2}", _bonusObject.Title, _bonusObject.Description, _bonusObject.DateOfRegistration.ToString("dd.MM.yyyy"));
            if (_bonusObject.IsUsed)
            {
                string activateDate = _bonusObject.DateOfUsed?.ToString("dd.MM.yyyy");
                message += string.Format("\nБонус було успішно активовано: {0}", activateDate);
            }

            var messageObj = _botClient.SendPhoto(chatId: _userContext.User.TelegramId, caption: message, photo: _bonusObject.CoverUrl, parseMode: "HTML");
            _sendedMessages.Add(new SendedMessageModel(messageObj.MessageId, deleteMessageMethodEnum));
        }

        void SendBonusInfoActions()
        {
            SendMessageArgs mes = new SendMessageArgs(_userContext.User.TelegramId, "Чи хочеш використати бонус?") { ParseMode = "HTML" };
            mes.ReplyMarkup = GenerateBonusInfoKeyboard();

            var messageObj = _botClient.SendMessage(mes);
            _sendedMessages.Add(new SendedMessageModel(messageObj.MessageId, DeleteMessageMethodEnum.ClosePage));
        }

        void ProblemWithGetDataFromServerMessage()
        {
            SendMessageArgs mes = new SendMessageArgs(_userContext.User.TelegramId, Widjets.ProblemWithExternalServer()) { ParseMode = "HTML" };
            var messageObj = _botClient.SendMessage(mes);
            _sendedMessages.Add(new SendedMessageModel(messageObj.MessageId, DeleteMessageMethodEnum.None));
        }

        private InlineKeyboardMarkup GenerateBonusInfoKeyboard()
        {
            var yesBtn = new InlineKeyboardButton(text: _userContext.ResourceManager.GetString("Spend"));

            var replyMarkuppp = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                        new InlineKeyboardButton[]{
                            yesBtn,
                         }
                }
            );

            yesBtn.CallbackData = "ok";

            return replyMarkuppp;
        }

    }
}
