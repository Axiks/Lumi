using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.UI;

namespace Vanilla.TelegramBot.Abstract
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

        public int? _catalogInitMessageId;

        public List<SendedMessageModel> _sendedMessages;

        bool _isWithCannelButton;

        readonly string _deliver = " mya~ ";

        string? _folderName;


        public AbstractFolder(TelegramBotClient botClient, UserContextModel userContext, IUserService userService, ILogger logger, bool isWithCannelButton = true, string folderName = null)
        {
            _logger = logger;
            _userContext = userContext;
            _botClient = botClient;
            _userService = userService;
            _isWithCannelButton = isWithCannelButton;

            _folderName = folderName;

            _index = 0;
            _sendMessages = new List<int>();
            _sendedMessages = new List<SendedMessageModel>();

            _userContext.UpdateUserContext = _userContext.UpdateUserContext is null ? new BotUpdateUserModel() : _userContext.UpdateUserContext;
            _userModel = _userContext.UpdateUserContext;

        }

        public void InitPagesCatalog(List<IPage> pagesCatalog)
        {
            _pagesCatalog = pagesCatalog;
        }

        public void InitPages(List<IPage> pages)
        {
            var initPages = new List<IPage>();

            foreach (var page in pages)
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
            ReplyKeyboardMarkup replyKeyboard = null;
            if (_isWithCannelButton is true) replyKeyboard = Keyboards.BackKeyboard(_userContext);

            //var taskName = _folderName is not null ? _folderName : "Run task";
            var taskName = _folderName is not null ? _folderName : new EmojiRandomizer().Take();
            var mess = _botClient.SendMessage(_userContext.User.TelegramId, taskName, replyMarkup: replyKeyboard, parseMode: "HTML");

            _catalogInitMessageId = mess.MessageId;
            //_sendMessages.Add(mess.MessageId);

            //var respnse = _botClient.EditMessageText(chatId: _userContext.User.TelegramId, messageId: mess.MessageId, text: "EditrdMessage", replyMarkup: Keyboards.CannelKeyboard(_userContext));

            ApplayPage();
        }

        void PageNotifiedComplite() => NextPage();
        public virtual void EnterPoint(Update update)
        {
            if(CannelActionMidleware(update) == true) return;
            AutoRemoveNextActionMessages();

            if (Helpers.ValidatorHelpers.InlineBtnActionValidate(update, _userContext.ResourceManager.GetString("Back")))
            {
                var myCastedObject = _pages[_index] as IPageKeyboardExtension;
                if (myCastedObject != null)
                {
                    var changeFlowExtension = (IPageKeyboardExtension)_pages[_index];
                    if (changeFlowExtension.BackBtnIntercept is false)
                    {
                        _sendMessages.Add(update.Message.MessageId);
                        BackPage();
                        return;
                    }
                    else
                    {
                        _botClient.DeleteMessage(_userContext.User.TelegramId, update.Message.MessageId);
                    }
                }
                else
                {
                    _sendMessages.Add(update.Message.MessageId);
                    BackPage();
                    return;
                }

            }
            _pages[_index].InputHendler(update);
        }

        public bool CannelActionMidleware(Update update) {
            if (update.CallbackQuery is null) return false;
            if (update.CallbackQuery.Data != "cannel") return false;

            CloseFolder();

            return true;
        }

        internal void ApplayPage()
        {
            AutoRemoveMessagesFromPage();
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
            AutoRemoveNextPageMessages();
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

        void BackPage()
        {
            if (_index > 0)
            {
                _index--;
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

        void PutPagesToFlow(List<IPage> pages)
        {
            UnubscribePageEvents(); //Fix

            List<IPage> newLists = pages;

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

            var exeptionId = _logger.WriteLog(ex.Message, LogType.Error, UserId: _userContext.User.UserId);

            var errorMessage = string.Format(_userContext.ResourceManager.GetString("ServerError"), "@Yumikki", exeptionId);
        }

        void SubscribePageEvents()
        {
            _pages[_index].CompliteEvent += PageNotifiedComplite;
            _pages[_index].ValidationErrorEvent += ValidationErrorEvent;
            _pages[_index].ChangePagesFlowPagesEvent += PutPagesToFlow;

            var myCastedObject = _pages[_index] as IPageChangeFlowExtension;
            if (myCastedObject != null)
            {
                var changeFlowExtension = (IPageChangeFlowExtension)_pages[_index];
                changeFlowExtension.ChangePagesFlowByPagesPagesEvent += PutPagesToFlow;
            }

            var myCastedObject2 = _pages[_index] as IPageKeyboardExtension;
            if (myCastedObject2 != null)
            {
                var changeFlowExtension = (IPageKeyboardExtension)_pages[_index];
                changeFlowExtension.ChangeInlineKeyboardEvent += ChangeInitKeyboard;
                /*changeFlowExtension.ChangeInlineKeyboardEvent += async (keyboard) =>
                {
                    await ChangeInitKeyboard(keyboard);
                };*/
            }
        }

        void UnubscribePageEvents()
        {
            _pages[_previousIndex].CompliteEvent -= PageNotifiedComplite;
            _pages[_previousIndex].ValidationErrorEvent -= ValidationErrorEvent;
            _pages[_previousIndex].ChangePagesFlowPagesEvent -= PutPagesToFlow;

            var myCastedObject = _pages[_index] as IPageChangeFlowExtension;
            if (myCastedObject != null)
            {
                var changeFlowExtension = (IPageChangeFlowExtension)_pages[_index];
                changeFlowExtension.ChangePagesFlowByPagesPagesEvent -= PutPagesToFlow;
            }

            var myCastedObject2 = _pages[_index] as IPageKeyboardExtension;
            if (myCastedObject2 != null)
            {
                var changeFlowExtension = (IPageKeyboardExtension)_pages[_index];
                changeFlowExtension.ChangeInlineKeyboardEvent -= ChangeInitKeyboard;
                /* changeFlowExtension.ChangeInlineKeyboardEvent -= async (keyboard) =>
                 {
                     await ChangeInitKeyboard(keyboard);
                 };*/
            }
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

        void ChangeInitKeyboard(ReplyKeyboardMarkup replyKeyboardMarkup)
        {
            //var respnse = _botClient.EditMessageReplyMarkup(chatId: _userContext.User.TelegramId, messageId: _catalogInitMessageId ?? 0, replyMarkup: Keyboards.BackKeyboard(_userContext));    
        }

        void AutoRemoveNextActionMessages()
        {
            if (_sendedMessages.Where(x => x.method == DeleteMessageMethodEnum.NextAction).Count() > 0)
            {
                var messagesToRemove = _sendedMessages.Where(x => x.method == DeleteMessageMethodEnum.NextAction).Select(x => x.messageId);
                _botClient.DeleteMessages(_userContext.User.TelegramId, messagesToRemove);
            }
        }

        void AutoRemoveNextPageMessages()
        {
            if (_sendedMessages.Where(x => x.method == DeleteMessageMethodEnum.NextMessage).Count() > 0)
            {
                var messagesToRemove = _sendedMessages.Where(x => x.method == DeleteMessageMethodEnum.NextMessage).Select(x => x.messageId);
                _botClient.DeleteMessages(_userContext.User.TelegramId, messagesToRemove);
            }
        }

        void AutoRemoveMessagesFromPage()
        {
            if (_sendedMessages.Where(x => x.method == DeleteMessageMethodEnum.ClosePage).Count() > 0)
            {
                var messagesToRemove = _sendedMessages.Where(x => x.method == DeleteMessageMethodEnum.ClosePage).Select(x => x.messageId);
                _botClient.DeleteMessages(_userContext.User.TelegramId, messagesToRemove);
            }
        }


        void ExitFromFolder()
        {
            if (_sendedMessages.Exists(x => x.method != DeleteMessageMethodEnum.None))
            {
                _botClient.DeleteMessages(_userContext.User.TelegramId, _sendedMessages.Where(x => x.method != DeleteMessageMethodEnum.None).Select(x => x.messageId));
            }

            ClearMessages();
            if (_catalogInitMessageId is not null) _botClient.DeleteMessage(_userContext.User.TelegramId, _catalogInitMessageId ?? 0);

            CloseFolderEvent.Invoke();
            _logger.WriteLog("Success exit from folder", LogType.Information, UserId: _userContext.User.UserId);
        }

        public void CloseFolder() => ExitFromFolder();

/*        string RandomEmojies()
        {



            string GetRandomEmoji =>;



        }*/

        /*        public void GoToPage(string path)
                {
                    var pageName = path.Split(_deliver).First();
                    var page = _pagesCatalog.First(x => x.GetType().Name == pageName);

                    List<IPage> newLists = new List<IPage>();

                    for (int i = 0; i < pagesRoute.Count(); i++)
                    {
                        var page = _pagesCatalog.First(x => x.GetType().Name == pagesRoute[i]);
                        newLists.Add(page);
                    }
                }*/
    }

    class EmojiRandomizer(int count = 3)
    {
        Random random = new Random();

        List<string> smiles = new List<string>
            {
                "😺", "😸", "😹", "😻", "😽", "😼", "🙀", "🐱"
            };
        string TakeEmoji() => smiles[random.Next(0, smiles.Count)];
        public string Take()
        {
            var emojiesString = "";
            for (int i = 0; i < count; i++)
            {
                emojiesString += " ";
                emojiesString += TakeEmoji();
            }
            return emojiesString;
        }
    }

}
