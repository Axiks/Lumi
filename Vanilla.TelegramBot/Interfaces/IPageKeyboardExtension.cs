using Telegram.BotAPI.AvailableTypes;

namespace Vanilla.TelegramBot.Interfaces
{
    public delegate void ChangeInlineKeyboardHandler(ReplyKeyboardMarkup replyKeyboardMarkup);
    internal interface IPageKeyboardExtension
    {
        public bool BackBtnIntercept { get; }
        public event ChangeInlineKeyboardHandler? ChangeInlineKeyboardEvent;
    }
}
