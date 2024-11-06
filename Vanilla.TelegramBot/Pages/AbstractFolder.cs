using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.UI;

namespace Vanilla.TelegramBot.Pages
{
    public abstract class AbstractFolder : IFolder
    {
        readonly ILogger _logger;

        readonly TelegramBotClient _botClient;
        readonly UserContextModel _userContext;
        readonly IUserService _userService;

        List<IPage> _pagesCatalog;
        //public abstract List<IPage> _pagesCatalog { get; }

        List<(short, short, List<IPage>)> _pagesVolumes;
        internal List<int> _sendMessages;

        short _previousIndex; // can be event bug
        short _index;
        List<IPage> _pages;

        private readonly BotUpdateUserModel _userModel;

        public event CloseFolderEventHandler? CloseFolderEvent;

        int? _catalogInitMessageId;


        public AbstractFolder(TelegramBotClient botClient, UserContextModel userContext, IUserService userService, ILogger logger)
        {
            _logger = logger;
            _userContext = userContext;
            _botClient = botClient;
            _userService = userService;

            _index = 0;
            _sendMessages = new List<int>();

            _userContext.UpdateUserContext = _userContext.UpdateUserContext is null ? new BotUpdateUserModel() : _userContext.UpdateUserContext;
            _userModel = _userContext.UpdateUserContext;

      /*      _pagesCatalog = new List<IPage>
            {
                new UpdateUserNicknamePage(_botClient, _userContext, _sendMessages, _userModel),
                new UpdateUserAboutPage(_botClient, _userContext, _sendMessages, _userModel),
                new UpdateUserLinksPage(_botClient, _userContext, _sendMessages, _userModel),
                new UpdateIsRedyToWorkPage(_botClient, _userContext, _sendMessages, _userModel),
                new UpdateUserImagesPage(_botClient, _userContext, _sendMessages, _userModel),
                new UpdateUserComplitePage(_botClient, _userContext, _sendMessages, _userModel, _userService),
                new UpdateSeccessUserPage(_botClient, _userContext, _sendMessages, _userModel),
            };*/

            /*_pagesVolumes = new List<(short, short, List<IPage>)>
            {
                new (0, 0, new List<IPage>(_pagesCatalog))
            };*/

        }

        public void InitPagesCatalog(List<IPage> pagesCatalog)
        {
            _pagesCatalog = pagesCatalog;
        }

        public void InitPages(List<IPage> pages)
        {
            var initPages = new List<IPage>();

            foreach(var page in pages)
            {
                //_pagesCatalog.First(x => x == page);
                initPages.Add(_pagesCatalog.First(x => x == page));
            }

            _pagesVolumes = new List<(short, short, List<IPage>)>
            {
                new (0, 0, new List<IPage>(initPages))
            };


            LoadVolume();
            //ApplayPage();
        }

        public void Run()
        {
            var mess = _botClient.SendMessage(_userContext.User.TelegramId, "Run task", replyMarkup: Keyboards.CannelKeyboard(_userContext), parseMode: "HTML");
            _catalogInitMessageId = mess.MessageId;
            //_sendMessages.Add(mess.MessageId);

            ApplayPage();
        }

        void PageNotifiedComplite() => NextPage();
        public virtual void EnterPoint(Update update) => _pages[_index].InputHendler(update);

        internal void ApplayPage()
        {
            ClearMessages();
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

        public void NextPage()
        {
            if (_index < (short)_pages.Count() - 1)
            {
                _index++;
                ApplayPage();
            }
            else
            {
                // Try change volume
                if (_pagesVolumes.Count() > 1)
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

        void PutPagesToFlow(List<string> pagesRoute)
        {
            UnubscribePageEvents(); //Fix

            List<IPage> newLists = new List<IPage>();

            for (int i = 0; i < pagesRoute.Count(); i++)
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

        void ValidationErrorEvent(string text) => MessageSendHelper("Validation Error\n\n" + text);
        void InternallException(Exception ex)
        {

            var exeptionId = _logger.WriteLog(ex.Message, LogType.Error);

            var errorMessage = string.Format(_userContext.ResourceManager.GetString("ServerError"), "@Yumikki", exeptionId);
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
            if (_sendMessages.Count() <= 0) return;
            _botClient.DeleteMessages(_userContext.User.TelegramId, _sendMessages);
            _sendMessages.Clear();
        }

        void LoadVolume() => (_index, _previousIndex, _pages) = _pagesVolumes.Last();

        void ExitFromFolder()
        {
            ClearMessages();
            if(_catalogInitMessageId is not null) _botClient.DeleteMessage(_userContext.User.TelegramId, _catalogInitMessageId ?? 0);
            CloseFolderEvent.Invoke();
            Console.WriteLine("Success exit from folder");
        }

        public void CloseFolder() => ExitFromFolder();
    }
}
