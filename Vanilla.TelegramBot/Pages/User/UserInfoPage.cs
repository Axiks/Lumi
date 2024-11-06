using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.UI;
using Vanilla.TelegramBot.UI.Widgets;

namespace Vanilla.TelegramBot.Pages.User
{
    internal class UserInfoPage : IPage
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
        public event ChangePagesFlowEventHandler? ChangePagesFlowPagesEvent;
        public event CompliteHandler? CompliteEvent;

        public TelegramBotClient _botClient;
        readonly UserContextModel _userContext;
        readonly UserModel _userInfo;
        readonly List<int> _sendMessages;
        public int? _inlineKeyboardId;
        public int? _inlineKeyboardUniqueId;

        //const string InitMessage = "<b>{0}</b>\n\n{1}\n\nЗамовлення: {2}\n\n<b>Доєднуйтесь до смоїх соцмереж:</b>\n\n{3}";


        public UserInfoPage(TelegramBotClient botClient, UserContextModel userContextModel, UserModel userInfo, List<int> sendMessages)
        {
            _botClient = botClient;
            _userContext = userContextModel;
            _userInfo = userInfo;
            _sendMessages = sendMessages;
        }

        void IPage.SendInitMessage()
        {
            var mess = _botClient.SendMessage(_userContext.User.TelegramId, "Мій профайллл", replyMarkup: Keyboards.ProfileKeyboard(_userContext), parseMode: "HTML");
            _inlineKeyboardId = mess.MessageId;
            //_inlineKeyboardUniqueId = mess.id
            _sendMessages.Add(mess.MessageId);

            string redyMessage = _userInfo.IsRadyForOrders ? "Reddy" : "No reddy";
            string links = _userInfo.Links is not null ? String.Join(", ", _userInfo.Links) : "No links";

            var message = Widjets.AboutUser(_userContext.ResourceManager, _userInfo);

            //MessageSendHelper(string.Format(InitMessage, _userInfo.Nickname, _userInfo.About, redyMessage, links));
            MessageSendHelper(message, _userInfo.Images);

            //CompliteEvent.Invoke();
        }

        void IPage.InputHendler(Update update)
        {
            //if(!ValidateInputType(update)) return;

            //Action(update);
        }

/*        bool ValidateInputType(Update update)
        {
            if (!ValidatorHelpers.InlineBtnActionValidate(update, _userContext.ResourceManager.GetString("MyProfileUpdate")))
            {
                ValidationErrorEvent.Invoke("Не те що очікувала. Обери дію!");
                return false;
            }
            return true;
        }

        void Action(Update update)
        {
            if(ValidatorHelpers.InlineBtnActionValidate(update, _userContext.ResourceManager.GetString("MyProfileUpdate")))
            {
                ChangePagesFlowPagesEvent
            }
        }*/

        public void MessageSendHelper(string text, List<ImageModel>? imagesIist = null)
        {
            //var mess = _botClient.SendMessage(_userContext.User.TelegramId, text, parseMode: "HTML");
            //_sendMessages.Add(mess.MessageId);

            var mediaList = new List<InputMedia>();

            if (imagesIist is not null)
            {
                foreach (var img in imagesIist)
                {
                    var inputPhoto = new InputMediaPhoto(img.TgMediaId);
                    mediaList.Add(inputPhoto);
                }

                if (imagesIist.Count() > 0)
                {
                    mediaList.First().Caption = text;
                    mediaList.First().ParseMode = "HTML";
                }

            }


            if (mediaList.Count() > 0)
            {
                /*var mess = _botClient.SendMessage(chatId: _userContext.User.TelegramId,  text: "Мій профайл", replyMarkup: Keyboards.ProfileKeyboard(_userContext), parseMode: "HTML");
                _sendMessages.Add(mess.MessageId);*/

                //if (_inlineKeyboardId is not null) _botClient.EditMessageText(chatId: _userContext.User.TelegramId, messageId: _inlineKeyboardId ?? 0, text: "UserInfoKb", parseMode: "HTML");

                var groups = _botClient.SendMediaGroup(_userContext.User.TelegramId, mediaList);
                _sendMessages.AddRange(groups.Select(x => x.MessageId).ToList());
            }
            else
            {;
                var mess = _botClient.SendMessage(_userContext.User.TelegramId, text, replyMarkup: Keyboards.ProfileKeyboard(_userContext), parseMode: "HTML");
                _sendMessages.Add(mess.MessageId);
            }
        }
    }
}
