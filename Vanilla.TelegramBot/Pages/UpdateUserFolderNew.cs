using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages.UpdateUser;
using Vanilla_App.Models;

namespace Vanilla.TelegramBot.Pages
{
    public class UpdateUserFolderNew : IFolder
    {

        readonly ILogger _logger;

        readonly TelegramBotClient _botClient;
        readonly UserContextModel _userContext;
        readonly IUserService _userService;
        short _index;
        short _previousndex;
        readonly List<IPage> _pages;
        readonly List<int> _sendMessages;
        private bool _isReadyMoveToNextPage;

        private readonly BotUpdateUserModel _userModel;
        public UpdateUserFolderNew(TelegramBotClient botClient, UserContextModel userContext, IUserService userService, ILogger logger)
        {
            _logger = logger;
            _userContext = userContext;
            _botClient = botClient;
            _userService = userService;


            _previousndex = 0;
            _index = 0;
            _sendMessages = new List<int>();
            _isReadyMoveToNextPage = true;

            _userContext.UpdateUserContext = _userContext.UpdateUserContext is null ? new BotUpdateUserModel() : _userContext.UpdateUserContext;
            _userModel = _userContext.UpdateUserContext;

            _pages = new List<IPage>
            {
                new UpdateUserNicknamePage(_botClient, userContext, _sendMessages, _userModel),
                new UpdateUserAboutPage(_botClient, userContext, _sendMessages, _userModel),
                new UpdateUserLinksPage(_botClient, userContext, _sendMessages, _userModel),
                new UpdateIsRedyToWorkPage(_botClient, userContext, _sendMessages, _userModel),
                new UpdateUserComplitePage(_botClient, userContext, _sendMessages, _userModel, _userService),
                new UpdateSeccessUserPage(_botClient, userContext, _sendMessages, _userModel),
            };

            ApplayPage();
        }
        short IFolder.CurrentPageIndex => _index;
        short IFolder.NumberOfPages => (short)_pages.Count();

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
            if (index < (short)_pages.Count()) _index = index;
            ApplayPage();
        }

        public void NextPage()
        {
            if (!_isReadyMoveToNextPage) return;

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
            catch (Exception ex)
            {
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

            var errorMessage = string.Format(_userContext.ResourceManager.GetString("ServerError"), "@Yumikki", exeptionId);
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
            var mess = _botClient.SendMessage(_userContext.User.TelegramId, text);
            _sendMessages.Add(mess.MessageId);
        }

        /*        void ToCompliteAction()
                {

                }*/

        void ClearMessages() => _botClient.DeleteMessages(_userContext.User.TelegramId, _sendMessages);
    }
}
