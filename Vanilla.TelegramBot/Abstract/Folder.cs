using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Abstract
{
    public class Folder : IFolder
    {
        readonly ILogger _logger;

        readonly TelegramBotClient _botClient;
        readonly UserContextModel _userContext;
        short _index;
        short _previousndex;
        readonly List<IPage> _pages;
        readonly List<int> _sendMessages;
        private bool _isReadyMoveToNextPage;
        public Folder(List<IPage> pages, TelegramBotClient botClient, UserContextModel userContext, ILogger logger)
        {
            _logger = logger;
            _userContext = userContext;
            _botClient = botClient;

            _pages = pages;

            _previousndex = 0;
            _index = 0;
            _sendMessages = new List<int>();
            _isReadyMoveToNextPage = true;

            ApplayPage();
        }
        short IFolder.CurrentPageIndex => _index;
        short IFolder.NumberOfPages => (short) _pages.Count();

        event CloseFolderEventHandler IFolder.CloseFolderEvent
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

        void IFolder.CloseFolder()
        {
            throw new NotImplementedException();
        }

        void IFolder.GoToPage(short index)
        {
            if(index < (short)_pages.Count()) _index = index;
            ApplayPage();
        }

        public void NextPage()
        {
            if (_index < (short)_pages.Count()) _index++;
            ApplayPage();
        }

        void IFolder.PreviousPage()
        {
            if (_index > 0) _index--;
            ApplayPage();
        }

        void ApplayPage()
        {
            UnubscribePageEvents();
            SubscribePageEvents();
            try
            {
                _pages[_index].SendInitMessage();
            }
            catch (Exception ex) {
                InternallException(ex);
            }

            _previousndex = _index;

        }
        void IFolder.EnterPoint(Update update)
        {
            _isReadyMoveToNextPage = true;
            try
            {
                _pages[_index].InputHendler(update);
                NextPage();
            }
            catch (Exception ex)
            {
                InternallException(ex);
            }
        }

        void ValidationErrorEvent(string text)
        {
            _isReadyMoveToNextPage = false;
            MessageSendHelper("Validation Error\n\n" + text);
        }
        void InternallException(Exception ex)
        {
            _isReadyMoveToNextPage = false;

            var exeptionId = _logger.WriteLog(ex.Message, LogType.Error);

            var errorMessage = String.Format(_userContext.ResourceManager.GetString("ServerError"), "@Yumikki", exeptionId);
            MessageSendHelper(errorMessage);
        }
        void SubscribePageEvents()
        {
            _pages[_index].ValidationErrorEvent += ValidationErrorEvent;
        }

        void UnubscribePageEvents()
        {
            _pages[_previousndex].ValidationErrorEvent -= ValidationErrorEvent;
        }

        void MessageSendHelper(string text)
        {
            var mess = _botClient.SendMessage(_userContext.User.TelegramId, text, parseMode: "HTML");
            _sendMessages.Add(mess.MessageId);
        }

/*        void ToCompliteAction()
        {

        }*/

        void ClearMessages() => _botClient.DeleteMessages(_userContext.User.TelegramId, _sendMessages);
    }
}
