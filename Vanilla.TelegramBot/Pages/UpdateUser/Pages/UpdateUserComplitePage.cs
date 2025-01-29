using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.Extensions;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.UI;
using Vanilla.TelegramBot.UI.Widgets;
namespace Vanilla.TelegramBot.Pages.UpdateUser.Pages
{
    internal class UpdateUserComplitePage(TelegramBotClient _botClient, UserContextModel _userContext, List<int> _sendMessages, IUserService _userService, UserActionContextModel _updateUserActionContextModel) : IPage, IPageKeyboardExtension
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
        public event ChangePagesFlowEventHandler? ChangePagesFlowPagesEvent;
        public event CompliteHandler? CompliteEvent;
        public event ChangeInlineKeyboardHandler? ChangeInlineKeyboardEvent;

        private int _initMessageId;

        private bool _backBtnIntercept = true;


        string _basicMessage
        {
            get
            {
                return Widjets.AboutUser(_userContext.ResourceManager, _updateUserActionContextModel);
            }
        }

        public bool BackBtnIntercept => _backBtnIntercept;

        void IPage.SendInitMessage() => MessageSendHelper(_basicMessage, _updateUserActionContextModel.Images);

        void IPage.InputHendler(Update update)
        {
            if (!ValidateInputType(update)) return;
            Router(update);
        }

        bool ValidateInputType(Update update)
        {
            if (update.CallbackQuery is not null)
            {
                return true;
            }
            else if (Helpers.ValidatorHelpers.InlineBtnActionValidate(update, _userContext.ResourceManager.GetString("Back"))) return true;

            ValidationErrorEvent.Invoke("Не те що очікувала. Обери дію з кнопки!");
            return false;
        }

        void Router(Update update)
        {
            if (Helpers.ValidatorHelpers.InlineBtnActionValidate(update, _userContext.ResourceManager.GetString("Back")))
            {
                _backBtnIntercept = false;
                BackKeyboardAction(update);
                return;
            }
            else if (update.CallbackQuery.Data == "true")
            {
                SaveUserAction(update);
                CompliteEvent.Invoke();
            }
            else if (update.CallbackQuery.Data == "edit") UpdateKeyboardAction(update);
            else if (update.CallbackQuery.Data == "back") BackKeyboardAction(update);
            else if (update.CallbackQuery.Data == "nickname") ReturnToPage("UpdateUserNicknamePage");
            else if (update.CallbackQuery.Data == "about") ReturnToPage("UpdateUserAboutPage");
            else if (update.CallbackQuery.Data == "isRedyToWork") ReturnToPage("UpdateIsRedyToWorkPage");
            else if (update.CallbackQuery. Data == "links") ReturnToPage("UpdateUserLinksPage");
            else if (update.CallbackQuery.Data == "images") ReturnToPage("UpdateUserImagesPage");
            else
            {
                throw new Exception("Неочікувана дія");
            }

            //Action(update);
            //UpdateKeyboard(update);
        }

        async void SaveUserAction(Update update)
        {
            if (_updateUserActionContextModel.Images is not null && _updateUserActionContextModel.Images.Count() > 0) SaveImages(update);

            //_userContext.User.IsHasProfile = true;
            _userContext.Roles.Add(RoleEnum.User);

            //fix
            try
            {
                var user = await _userService.SignInUserAsync(_userContext.UpdateUser.TgId);


                var userModel = await _userService.UpdateUserAsync(_userContext.UpdateUser.TgId, new UserUpdateRequestModel
                {
                    Nickname = _updateUserActionContextModel.Nickname,
                    Links = _updateUserActionContextModel.Links,
                    About = _updateUserActionContextModel.About,
                    IsRadyForOrders = _updateUserActionContextModel.IsRadyForOrders,
                    Images = _updateUserActionContextModel.Images,
                    IsHasProfile = _userContext.Roles.Contains(RoleEnum.User)
                });

                _userContext.User = userModel; // update user model after save
            }
            catch (Exception ex) { 
                
            }

        }

        void SaveImages(Update update)
        {
            _botClient.SendChatAction(_userContext.UpdateUser.TgId, "upload_document");

            foreach (var image in _updateUserActionContextModel.Images)
            {
                var file = _botClient.GetFile(image.TgMediaId);
                var imageUrl = _botClient.BuildFileDownloadLink(file);

                image.DownloadPath = imageUrl;
            }
        }


        void BackKeyboardAction(Update update)
        {
            _botClient.EditMessageText(chatId: _userContext.UpdateUser.TgId, messageId: _initMessageId, text: "Чи усе заповнено вірно?", replyMarkup: Keyboards.GetBasicKeypoard(_userContext.ResourceManager), parseMode: "HTML");
            _backBtnIntercept = false;
        }
        void UpdateKeyboardAction(Update update)
        {
            _botClient.EditMessageText(chatId: _userContext.UpdateUser.TgId, messageId: update.CallbackQuery.Message.MessageId, text: "Що саме оновити?", replyMarkup: Keyboards.GetSwitchPageKeypoard(_userContext.ResourceManager), parseMode: "HTML");
            _backBtnIntercept = true;
        }

        void ReturnToPage(string pageName)
        {
            var putFlow = new List<string> {
                pageName,
                //"UpdateUserComplitePage"
            };
            ChangePagesFlowPagesEvent.Invoke(putFlow);
            _backBtnIntercept = false;
            //CompliteEvent.Invoke();
        }

        void MessageSendHelper(string text, List<ImageModel>? imagesIist = null)
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


            if (mediaList is not null && mediaList.Count() > 0)
            {
                var groups = _botClient.SendMediaGroup(_userContext.UpdateUser.TgId, mediaList);
                foreach (var group in groups)
                {
                    _sendMessages.Add(group.MessageId);
                }

                var mess = _botClient.SendMessage(_userContext.UpdateUser.TgId, "Чи усе заповнено вірно?", replyMarkup: Keyboards.GetBasicKeypoard(_userContext.ResourceManager), parseMode: "HTML");
                _sendMessages.Add(mess.MessageId);
                _initMessageId = mess.MessageId;
            }
            else
            {
                var textPrefix = _userContext.Roles.Contains(RoleEnum.Anonim) is true ? "Бачу що усі дані було заповнено :3\nНа останок хочу переконатись що усе вірно запам'ятала\n\n" : "Чи усе заповнено вірно?\n\n";
                var mess = _botClient.SendMessage(_userContext.UpdateUser.TgId, textPrefix + text, replyMarkup: Keyboards.GetBasicKeypoard(_userContext.ResourceManager), parseMode: "HTML");
                _sendMessages.Add(mess.MessageId);
                _initMessageId = mess.MessageId;
            }

        }

    }

}
