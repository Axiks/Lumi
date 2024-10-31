using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Interfaces
{
    public delegate void ValidationErrorEventHandler(string ErrorMessage);
/*    internal delegate void NextPageEventHandler();
    internal delegate void PreviousPageEventHandler();*/

    public interface IPage
    {
        public event ValidationErrorEventHandler? ValidationErrorEvent;
/*        public event NextPageEventHandler? NextPageEvent;
        public event PreviousPageEventHandler? PreviousPageEvent;*/
        internal void SendInitMessage();
        internal void InputHendler(Update update);
    }
}
