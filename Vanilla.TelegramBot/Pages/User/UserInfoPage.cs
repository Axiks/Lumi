using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla.TelegramBot.Abstract;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.UI;
using Vanilla.TelegramBot.UI.Widgets;

namespace Vanilla.TelegramBot.Pages.User
{
    internal class UserInfoPage : BasicPageAbstract
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
        public event ChangePagesFlowEventHandler? ChangePagesFlowPagesEvent;
        public event CompliteHandler? CompliteEvent;

        public TelegramBotClient _botClient;
        readonly UserContextModel _userContext;
        readonly UserModel _userInfo;
        readonly List<SendedMessageModel> _sendMessages;
        public int? _inlineKeyboardId;
        public int? _inlineKeyboardUniqueId;

        public UserInfoPage(TelegramBotClient botClient, UserContextModel userContextModel, UserModel userInfo, List<SendedMessageModel> sendMessages) : base(botClient, userContextModel, sendMessages)
        {
            _botClient = botClient;
            _userContext = userContextModel;
            _userInfo = userInfo;
            _sendMessages = sendMessages;
        }

        public void MessageSendHelper(string text, List<ImageModel>? imagesIist = null)
        {
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
                var groups = _botClient.SendMediaGroup(_userContext.User.TelegramId, mediaList);

                foreach(var group in groups)
                {
                    AddMessage(group.MessageId, Common.Enums.DeleteMessageMethodEnum.ClosePage);
                }

            }
            else
            {;
                var mess = _botClient.SendMessage(_userContext.User.TelegramId, text, replyMarkup: Keyboards.ProfileKeyboard(_userContext), parseMode: "HTML");
                AddMessage(mess.MessageId, Common.Enums.DeleteMessageMethodEnum.ClosePage);
            }
        }

        public override void InitMessage()
        {
            var mess = _botClient.SendMessage(_userContext.User.TelegramId, "Мій профіль", replyMarkup: Keyboards.ProfileKeyboard(_userContext), parseMode: "HTML");
            _inlineKeyboardId = mess.MessageId;
            AddMessage(mess.MessageId, Common.Enums.DeleteMessageMethodEnum.ClosePage);

            string redyMessage = _userInfo.IsRadyForOrders ? "Reddy" : "No reddy";
            string links = _userInfo.Links is not null ? String.Join(", ", _userInfo.Links) : "No links";

            var message = Widjets.AboutUser(_userContext.ResourceManager, _userInfo);

            MessageSendHelper(message, _userInfo.Images);
        }

        public override void InitActions()
        {
        }
    }
}
