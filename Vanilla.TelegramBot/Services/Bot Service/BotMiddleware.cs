using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Services.Bot_Service
{
    public class BotMiddleware
    {
        public BotMiddleware(TelegramBotClient botClient, Update update, UserContextModel userContext) {
            if (update.Message is not null && update.Message.Photo is not null && update.Message.Photo.Count() > 0) userContext.UpdateLoadPhotoTimer();

            if (update.CallbackQuery is not null) botClient.AnswerCallbackQuery(update.CallbackQuery.Id);
        }
    }
}
