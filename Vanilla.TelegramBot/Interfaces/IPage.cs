using Telegram.BotAPI.GettingUpdates;

namespace Vanilla.TelegramBot.Interfaces
{
    public delegate void ValidationErrorEventHandler(string ErrorMessage);
    public delegate void ChangePagesFlowEventHandler(List<string> pages);
    public delegate void CompliteHandler();
/*    internal delegate void NextPageEventHandler();
    internal delegate void PreviousPageEventHandler();*/

    public interface IPage
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
        public event ChangePagesFlowEventHandler? ChangePagesFlowPagesEvent;
        public event CompliteHandler? CompliteEvent;
/*      public event NextPageEventHandler? NextPageEvent;
        public event PreviousPageEventHandler? PreviousPageEvent;*/
        internal void SendInitMessage();
        internal void InputHendler(Update update);
    }
}
