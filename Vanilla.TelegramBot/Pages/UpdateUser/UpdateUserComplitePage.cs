using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Pages.UpdateUser
{
    internal class UpdateUserComplitePage : IPage
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
        readonly TelegramBotClient _botClient;
        readonly UserContextModel _userContext;
        readonly List<int> _sendMessages;
        BotUpdateUserModel _dataContext;
        private readonly IUserService _userService;

        readonly string InitMessage = "Бачу що усі дані було заповнено:3\nНа останок хочу переконатись що усе вірно запам'ятала\n\n<b>{0}</b>\n{1}\nВиконую замовлення: {2}\n\nЗнайти мене можеш тут\n{3}";

        public UpdateUserComplitePage(TelegramBotClient botClient, UserContextModel userContext, List<int> sendMessages, BotUpdateUserModel dataContext, IUserService userService)
        {
            _botClient = botClient;
            _userContext = userContext;
            _sendMessages = sendMessages;
            _dataContext = dataContext;
            _userService = userService;
        }

        void IPage.SendInitMessage() => MessageSendHelper(string.Format(InitMessage, _dataContext.Nickname, _dataContext.About, _dataContext.IsRadyForOrders.ToString(), String.Join(", ", _dataContext.Links)));

        void IPage.InputHendler(Update update)
        {
            Action(update);
        }

        void Action(Update update)
        {
            _userService.UpdateUser(_userContext.User.TelegramId, new Models.UserUpdateRequestModel
            {
                Nickname = _dataContext.Nickname,
                Links = _dataContext.Links,
                About = _dataContext.About,
                IsRadyForOrders = _dataContext.IsRadyForOrders,
            });

            var userModel = _userService.GetUser(_userContext.User.UserId).Result; // Don`t get or save (
            //var messageContent = MessageWidgets.AboutUser(userModel);

            //messageContent += "\n I successfully update your profile :3\"";
        }

        void MessageSendHelper(string text)
        {
            var mess = _botClient.SendMessage(_userContext.User.TelegramId, text, replyMarkup: GetKeypoardNew(), parseMode: "HTML");
            _sendMessages.Add(mess.MessageId);
        }

        ReplyKeyboardMarkup GetKeypoard()
        {
            KeyboardButton[][] mainMenuKeyboardButtons = new KeyboardButton[][]{
                new KeyboardButton[]{
                    new KeyboardButton("Усе вірно"),
                    },// column 1
                    new KeyboardButton[]{
                        new KeyboardButton("Виправити")
                     }
            };

            return new(mainMenuKeyboardButtons);
        }

        InlineKeyboardMarkup GetKeypoardNew()
        {
            var acceptBtn = new InlineKeyboardButton(text: _userContext.ResourceManager.GetString("True"));
            var editBtn = new InlineKeyboardButton(text: _userContext.ResourceManager.GetString("Edit"));

            acceptBtn.CallbackData = "True";
            editBtn.CallbackData = "Edit";

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
    }
}
