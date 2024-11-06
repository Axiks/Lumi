﻿using System.Data;
using System.Text.Json;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.UI.Widgets;
namespace Vanilla.TelegramBot.Pages.UpdateUser
{
    internal class UpdateUserComplitePage : IPage
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
        public event ChangePagesFlowEventHandler? ChangePagesFlowPagesEvent;
        public event CompliteHandler? CompliteEvent;

        readonly TelegramBotClient _botClient;
        readonly UserContextModel _userContext;
        readonly List<int> _sendMessages;
        //BotUpdateUserModel _dataContext;
        private readonly IUserService _userService;

        readonly string InitMessage = "<b>{0}</b>\n{1}\n{2}\n\nЗнайти мене можеш тут\n{3}";

        public UpdateUserComplitePage(TelegramBotClient botClient, UserContextModel userContext, List<int> sendMessages, IUserService userService)
        {
            _botClient = botClient;
            _userContext = userContext;
            _sendMessages = sendMessages;
            _userService = userService;
        }
        
        string _basicMessage
        {
            get {
                //UserModel userModel =  (UserModel)UserModel.User.
                //var userModel = JsonSerializer.Deserialize<UserModel>(JsonSerializer.Serialize(_userContext.User));
                //userModel.Nickname = _dataContext.Nickname ?? userModel.Nickname;
                //userModel.About = _dataContext.About ?? userModel.About;
                //userModel.Links = _dataContext.Links ?? userModel.Links;
                //userModel.IsRadyForOrders = _dataContext.IsRadyForOrders ?? userModel.IsRadyForOrders;

                //return Widjets.AboutUser(_userContext.ResourceManager, userModel);
                return Widjets.AboutUser(_userContext.ResourceManager, _userContext.User);
            }
        }

        void IPage.SendInitMessage() => MessageSendHelper(_basicMessage, _userContext.User.Images);

        void IPage.InputHendler(Update update)
        {
            if (!ValidateInputType(update)) return;
            Router(update);
        }

        bool ValidateInputType(Update update)
        {
            if (update.CallbackQuery is null)
            {
                ValidationErrorEvent.Invoke("Не те що очікувала. Обери дію з кнопки!");
                return false;
            }
            return true;
        }

        void Router(Update update)
        {
            if (update.CallbackQuery.Data == "true")
            {
                InitAction(update);
                CompliteEvent.Invoke();
            }
            else if (update.CallbackQuery.Data == "edit") UpdateKeyboardAction(update);
            else if ((update.CallbackQuery.Data == "back")) BackKeyboardAction(update);
            else if ((update.CallbackQuery.Data == "nickname")) ReturnToPage("UpdateUserNicknamePage");
            else if ((update.CallbackQuery.Data == "about")) ReturnToPage("UpdateUserAboutPage");
            else if ((update.CallbackQuery.Data == "isRedyToWork")) ReturnToPage("UpdateIsRedyToWorkPage");
            else if ((update.CallbackQuery.Data == "links")) ReturnToPage("UpdateUserLinksPage");
            else if ((update.CallbackQuery.Data == "images")) ReturnToPage("UpdateUserImagesPage");
            else
            {
                throw new Exception("Неочікувана дія");
            }
            //Action(update);
            //UpdateKeyboard(update);
        }

        void InitAction(Update update)
        {
            if (_userContext.User.Images is not null && _userContext.User.Images.Count() > 0) SaveImages(update);

            _userService.UpdateUser(_userContext.User.TelegramId, new Models.UserUpdateRequestModel
            {
                Nickname = _userContext.User.Nickname,
                Links = _userContext.User.Links,
                About = _userContext.User.About,
                IsRadyForOrders = _userContext.User.IsRadyForOrders,
                Images = _userContext.User.Images,
            });
        }

        void SaveImages(Update update)
        {
            _botClient.SendChatAction(_userContext.User.TelegramId, "upload_document");

            foreach (var image in _userContext.User.Images)
            {
                var file = _botClient.GetFile(image.TgMediaId);
                image.TgUrl = file.FilePath;
            }
        }

        void BackKeyboardAction(Update update)
        {
            _botClient.EditMessageText(chatId: _userContext.User.TelegramId, messageId: update.CallbackQuery.Message.MessageId, text: "Чи усе заповнено вірно?", replyMarkup: GetBasicKeypoard(), parseMode: "HTML");
        }
        void UpdateKeyboardAction(Update update)
        {
            _botClient.EditMessageText(chatId: _userContext.User.TelegramId, messageId: update.CallbackQuery.Message.MessageId, text: "Що саме оновити?", replyMarkup: GetSwitchPageKeypoard(), parseMode: "HTML");
        }

        void ReturnToPage(string pageName)
        {
            var putFlow = new List<string> {
                pageName,
                //"UpdateUserComplitePage"
            };
            ChangePagesFlowPagesEvent.Invoke(putFlow);
            //CompliteEvent.Invoke();
        }

        void MessageSendHelper(string text, List<ImageModel>? imagesIist = null)
        {
            var mediaList = new List<InputMedia>();

            if(imagesIist is not null)
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
                var groups = _botClient.SendMediaGroup(_userContext.User.TelegramId, mediaList);
                foreach (var group in groups)
                {
                    _sendMessages.Add(group.MessageId);
                }

                var mess = _botClient.SendMessage(_userContext.User.TelegramId, "Чи усе заповнено вірно?", replyMarkup: GetBasicKeypoard(), parseMode: "HTML");
                _sendMessages.Add(mess.MessageId);
            }
            else
            {
                var textPrefix = _userContext.User is not null ? "Бачу що усі дані було заповнено :3\nНа останок хочу переконатись що усе вірно запам'ятала\n\n" : "Чи усе заповнено вірно?\n\n";
                var mess = _botClient.SendMessage(_userContext.User.TelegramId, textPrefix + text, replyMarkup: GetBasicKeypoard(), parseMode: "HTML");
                _sendMessages.Add(mess.MessageId);
            }


            //



            //_botClient.SendPhoto(_userContext.User.TelegramId, images.);

            //var mess = _botClient.SendMessage(_userContext.User.TelegramId, text, replyMarkup: GetBasicKeypoard(), parseMode: "HTML");
            //_sendMessages.Add(mess.MessageId);
        }

        InlineKeyboardMarkup GetBasicKeypoard()
        {
            var acceptBtn = new InlineKeyboardButton(text: _userContext.ResourceManager.GetString("True"));
            var editBtn = new InlineKeyboardButton(text: _userContext.ResourceManager.GetString("Edit"));

            acceptBtn.CallbackData = "true";
            editBtn.CallbackData = "edit";

            var replyMarkuppp = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                    new InlineKeyboardButton[]{
                                                acceptBtn
                                            },
                    new InlineKeyboardButton[]{
                                                editBtn
                                            },
                }
            );

            return replyMarkuppp;
        }

        InlineKeyboardMarkup GetSwitchPageKeypoard()
        {
            var nickname = new InlineKeyboardButton(text: _userContext.ResourceManager.GetString("Nickname"));
            var about = new InlineKeyboardButton(text: _userContext.ResourceManager.GetString("AboutKey"));
            var isRedy = new InlineKeyboardButton(text: _userContext.ResourceManager.GetString("IsRedyToWork"));
            var links = new InlineKeyboardButton(text: _userContext.ResourceManager.GetString("Links"));
            var images = new InlineKeyboardButton(text: _userContext.ResourceManager.GetString("Images"));
            var back = new InlineKeyboardButton(text: _userContext.ResourceManager.GetString("Back"));

            nickname.CallbackData = "nickname";
            about.CallbackData = "about";
            isRedy.CallbackData = "isRedyToWork";
            links.CallbackData = "links";
            images.CallbackData = "images";
            back.CallbackData = "back";

            var replyMarkuppp = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                    new InlineKeyboardButton[]{
                        nickname,
                        about,              
                    },
                    new InlineKeyboardButton[]{
                        links,
                        images
                    },
                    new InlineKeyboardButton[]{
                        isRedy
                    },
                    new InlineKeyboardButton[]{
                        back
                    },
                }
            );

            return replyMarkuppp;
        }
    }
}
