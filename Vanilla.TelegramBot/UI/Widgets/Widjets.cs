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
            var InitMessage = "<b>{0}</b>\n\n{1}\n\n{2}\n\nЗнайти мене можеш тут:\n\n{3}\n\n";

            var links = new List<string>();
            if (userModel.Links is not null) links.AddRange(userModel.Links);
            if (userModel.Username is not null) links.Add("@" + userModel.Username);
            var linkStr = String.Join(", ", links);

            var text = string.Format(InitMessage, userModel.Nickname, userModel.About, userModel.IsRadyForOrders == true ? resourceManager.GetString("IAcceptOrders") : "", linkStr);

            return text;

        }
    }
}
