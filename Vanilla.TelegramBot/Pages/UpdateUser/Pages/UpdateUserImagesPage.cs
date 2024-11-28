using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.TelegramBot.Helpers;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.UI;

namespace Vanilla.TelegramBot.Pages.UpdateUser.Pages
{
    internal class UpdateUserImagesPage : IPage
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
        public event ChangePagesFlowEventHandler? ChangePagesFlowPagesEvent;
        public event CompliteHandler? CompliteEvent;

        readonly TelegramBotClient _botClient;
        readonly UserContextModel _userContext;
        readonly List<int> _sendMessages;

        readonly string InitMessage = "Покажи декілька світлин\n\n<i>Це можуть бути як і твої роботи, так і будь-які інші світлини котрі можна показати</i>";

        public UpdateUserImagesPage(TelegramBotClient botClient, UserContextModel userContext, List<int> sendMessages)
        {
            _botClient = botClient;
            _userContext = userContext;
            _sendMessages = sendMessages;
        }

        void IPage.SendInitMessage()
        {
            ReinitObj(); // fix
            MessageSendHelper(InitMessage, Keyboards.GetPassKeypoard(_userContext));
        }

        void IPage.InputHendler(Update update)
        {
            if (!ValidateInputType(update)) return;

            Router(update);
        }

        void ReinitObj()
        {
            //_dataContext.ImagesId = new List<string>();
            _userContext.User.Images = new List<ImageModel>();
            _userContext.FinishUploadingPhotosEvent += ToNextPage;
        }

        bool ValidateInputType(Update update)
        {
            if (update.Message is not null && update.Message.Photo is not null)
            {
                _sendMessages.Add(update.Message.MessageId);
                return true;
            }
            else if (update.CallbackQuery is not null && update.CallbackQuery.Data is not null) return true;

            return false;
        }

        void Router(Update update)
        {
            if (update.Message is not null && update.Message.Photo is not null)
            {
                AddImageId(update);
            }
            else if (update.CallbackQuery.Data == "pass") CompliteEvent.Invoke();
            else
            {
                throw new Exception("Неочікувана дія");
            }
        }

        void ToNextPage()
        {
            _userContext.FinishUploadingPhotosEvent -= ToNextPage;
            CompliteEvent.Invoke();
        }

        void AddImageId(Update update)
        {
/*            var x = update.Message.Photo;

            foreach(var i in x)
            {
                Console.WriteLine(i.FileId);
                Console.WriteLine(i.FileUniqueId);
                Console.WriteLine(i.FileSize);
                Console.WriteLine(i.Width);
                Console.WriteLine(i.Height);
                Console.WriteLine("\n");
            }*/

            var maxSizeImg = update.Message.Photo.OrderBy(x => x.FileSize).Last();
            var id = maxSizeImg.FileId;
            _userContext.User.Images.Add(new ImageModel { TgMediaId = id });

            /*imagesData.Remove(original);


            foreach (var image in imagesData)
            {
                _userContext.User.Images.Add(new ImageModel { TgMediaId = id });
            }

            var imagesData = update.Message.Photo.GroupBy(x => x.FileUniqueId).Last();
            var id = imagesData.First().FileId;
            _userContext.User.Images.Add(new ImageModel { TgMediaId = id });

            foreach (var image in imagesData)
            {
                var id = image.FileId;
                _dataContext.ImagesId.Add(id);
            }*/
        }

        void MessageSendHelper(string text, InlineKeyboardMarkup? keyboard = null)
        {
            var mess = _botClient.SendMessage(_userContext.User.TelegramId, text, replyMarkup: Keyboards.GetPassKeypoard(_userContext), parseMode: "HTML");
            _sendMessages.Add(mess.MessageId);
        }


    }
}
