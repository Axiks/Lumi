using System.Resources;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Vanilla.TelegramBot.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Vanilla.TelegramBot.UI.Widgets
{
    public static class Widjets
    {
        //public static string AboutUser(long chatId, TelegramBotClient botClient, ResourceManager resourceManager, UserModel userModel, ReplyMarkup? replyMarkup = null)
        public static string AboutUser(ResourceManager resourceManager, UserModel userModel)
        {
            var InitMessage = "<b>{0}</b>\n{1}\n{2}\n\nЗнайти мене можеш тут\n{3}";

            var links = new List<string>();
            if (userModel.Links is not null) links.AddRange(userModel.Links);
            if (userModel.Username is not null) links.Add("@" + userModel.Username);
            var linkStr = String.Join(", ", links);

            var text = string.Format(InitMessage, userModel.Nickname, userModel.About, userModel.IsRadyForOrders == true ? resourceManager.GetString("IAcceptOrders") : "", linkStr);

            return text;


            /* var _sendMessages = new List<int>();
            var mediaList = new List<InputMedia>();
            if (userModel.Images is not null)
            {
                foreach (var img in userModel.Images)
                {
                    var inputPhoto = new InputMediaPhoto(img.TgMediaId);
                    mediaList.Add(inputPhoto);
                }

                if (userModel.Images.Count() > 0)
                {
                    mediaList.First().Caption = text;
                    mediaList.First().ParseMode = "HTML";
                }

            }

            if (mediaList.Count() > 0) {
                var groups = botClient.SendMediaGroup(chatId, mediaList);
                foreach (var group in groups)
                {
                    _sendMessages.Add(group.MessageId);
                }
                var mess = botClient.SendMessage(chatId, "Чи усе заповнено вірно?", replyMarkup: replyMarkup, parseMode: "HTML");
                _sendMessages.Add(mess.MessageId);
            }
            else
            {
                var textPrefix = "Бачу що усі дані було заповнено :3\nНа останок хочу переконатись що усе вірно запам'ятала\n\n";
                var mess = botClient.SendMessage(chatId, textPrefix + text, replyMarkup: replyMarkup, parseMode: "HTML");
                _sendMessages.Add(mess.MessageId);
            }

            return _sendMessages;*/
        }
    }
}
