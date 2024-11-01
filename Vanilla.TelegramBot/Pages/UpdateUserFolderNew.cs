using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
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

        readonly List<IPage> _pagesCatalog;

        List<(short, short, List<IPage>)> _pagesVolumes;
        List<int> _sendMessages;

        short _previousIndex; // can be event bug
        short _index;
        List<IPage> _pages;
        //private bool _isReadyMoveToNextPage;


        private readonly BotUpdateUserModel _userModel;

        public event CloseFolderEventHandler CloseFolderEvent;

        public UpdateUserFolderNew(TelegramBotClient botClient, UserContextModel userContext, IUserService userService, ILogger logger)
        {
            _logger = logger;
            _userContext = userContext;
            _botClient = botClient;
            _userService = userService;


            _previousIndex = 0;
            _sendMessages = new List<int>();
            //_isReadyMoveToNextPage = true;

            _userContext.UpdateUserContext = _userContext.UpdateUserContext is null ? new BotUpdateUserModel() : _userContext.UpdateUserContext;
            _userModel = _userContext.UpdateUserContext;

            _pagesCatalog = new List<IPage>
            {
                new UpdateUserNicknamePage(_botClient, userContext, _sendMessages, _userModel),
                new UpdateUserAboutPage(_botClient, userContext, _sendMessages, _userModel),
                new UpdateUserLinksPage(_botClient, userContext, _sendMessages, _userModel),
                new UpdateIsRedyToWorkPage(_botClient, userContext, _sendMessages, _userModel),
                new UpdateUserImagesPage(_botClient, userContext, _sendMessages, _userModel),
                new UpdateUserComplitePage(_botClient, userContext, _sendMessages, _userModel, _userService),
                new UpdateSeccessUserPage(_botClient, userContext, _sendMessages, _userModel),
            };

            _pagesVolumes = new List<(short, short, List<IPage>)>
            {
                new (0, 0, new List<IPage>(_pagesCatalog))
            };

            LoadVolume();
            ApplayPage();
        }
        short IFolder.CurrentPageIndex => _index;
        short IFolder.NumberOfPages => (short)_pages.Count();


        void IFolder.CloseFolder()
        {
            throw new NotImplementedException();
        }

        void IFolder.GoToPage(short index)
        {
            if (index < (short)_pages.Count()) _index = index;
            ApplayPage();
        }

        void PageNotifiedComplite() => NextPage();
        public void NextPage()
        {
            //if (!_isReadyMoveToNextPage) return;

            if (_index < (short)_pages.Count() - 1)
            {
                _index++;
                ApplayPage();
            }
            else
            {
                // Try change volume
                if(_pagesVolumes.Count() > 1)
                {
                    _pagesVolumes.Remove(_pagesVolumes.Last());
                    LoadVolume(); // Load next volume
                    ApplayPage();
                }
                else
                {
                    // Exit from folder
                    // Maybe call event?
                    ExitFromFolder();
                    return;
                }
            }

        }

        void ExitFromFolder()
        {
            ClearMessages();
            CloseFolderEvent.Invoke();
            Console.WriteLine("Success exit from folder");
        }

        void IFolder.PreviousPage()
        {
            if (_index > 0) _index--;
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

            _previousIndex = _index;

        }
        void IFolder.EnterPoint(Update update)
        {
            //_isReadyMoveToNextPage = true;
            try
            {
                _pages[_index].InputHendler(update);
                //NextPage();
            }
            catch (Exception ex)
            {
                InternallException(ex);
            }
        }

        void PutPagesToFlow(List<string> pagesRoute)
        {
            UnubscribePageEvents(); //Fix

            List <IPage> newLists = new List <IPage>();

            for (int i = 0; i < pagesRoute.Count() ; i++)
            {
                var page = _pagesCatalog.First(x => x.GetType().Name == pagesRoute[i]);
                newLists.Add(page);
            }

            // Save current index state
            var current = _pagesVolumes.Last();
            current.Item1 = _index;
            current.Item1 = _previousIndex;

            var lastIndex = _pagesVolumes.Count() - 1;

            _pagesVolumes[lastIndex] = current;

            (short, short, List<IPage>) newVolume = (0, 0, newLists);
            _pagesVolumes.Add(newVolume);

            LoadVolume();
            ApplayPage();
        }

        void ValidationErrorEvent(string text)
        {
            //_isReadyMoveToNextPage = false;
            MessageSendHelper("Validation Error\n\n" + text);
        }
        void InternallException(Exception ex)
        {
            //_isReadyMoveToNextPage = false;

            var exeptionId = _logger.WriteLog(ex.Message, LogType.Error);

            var errorMessage = string.Format(_userContext.ResourceManager.GetString("ServerError"), "@Yumikki", exeptionId);
            MessageSendHelper(errorMessage);
        }
        void SubscribePageEvents()
        {
            _pages[_index].CompliteEvent += PageNotifiedComplite;
            _pages[_index].ValidationErrorEvent += ValidationErrorEvent;
            _pages[_index].ChangePagesFlowPagesEvent += PutPagesToFlow;
        }

        void UnubscribePageEvents()
        {
            _pages[_previousIndex].CompliteEvent -= PageNotifiedComplite;
            _pages[_previousIndex].ValidationErrorEvent -= ValidationErrorEvent;
            _pages[_previousIndex].ChangePagesFlowPagesEvent -= PutPagesToFlow;
        }

        void MessageSendHelper(string text)
        {
            var mess = _botClient.SendMessage(_userContext.User.TelegramId, text, parseMode: "HTML");
            _sendMessages.Add(mess.MessageId);
        }

        void ClearMessages()
        {
            _botClient.DeleteMessages(_userContext.User.TelegramId, _sendMessages);
            _sendMessages.Clear();
        }

        void LoadVolume() => (_index, _previousIndex, _pages) = _pagesVolumes.Last();
    }
}
