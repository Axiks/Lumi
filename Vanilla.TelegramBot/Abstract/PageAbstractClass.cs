using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.BotAPI;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Abstract
{
    public abstract class PageAbstractClass<T> : BasicFolderModel, IPage
    {
        readonly TelegramBotClient _botClient;
        readonly UserContextModel _userContext;
        readonly List<int> _sendMessages;
        T _dataContext;

        public event ChangePagesFlowEventHandler? ChangePagesFlowPagesEvent;
        public event CompliteHandler? CompliteEvent;

        public PageAbstractClass(TelegramBotClient botClient, UserContextModel userContext, List<int> sendMessages, T dataContext)
        {
            _botClient = botClient;
            _userContext = userContext;
            sendMessages = _sendMessages!;
            _dataContext = dataContext;
        }


        event ValidationErrorEventHandler? IPage.ValidationErrorEvent
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        void IPage.InputHendler(Update update)
        {
            ValidateInputType(update);
            ValidateInputData(update);

            // Як би то зупиняти потік виконання Хммм
            Action(update);
        }

        void IPage.SendInitMessage() => SendInitMessage();

        internal abstract void ValidateInputType(Update update);
        internal abstract void ValidateInputData(Update update);
        internal abstract void Action(Update update);
        internal abstract void SendInitMessage();

    }
}
