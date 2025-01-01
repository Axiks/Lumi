using Microsoft.IdentityModel.Abstractions;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.Extensions;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla.Common;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages.Bonus;
using Vanilla.TelegramBot.Pages.CreateUser;
using Vanilla.TelegramBot.Pages.Projects;
using Vanilla.TelegramBot.Pages.UpdateUser;
using Vanilla.TelegramBot.Pages.User;
using Vanilla.TelegramBot.UI;
using Vanilla_App.Services.Bonus;
using Vanilla_App.Services.Projects;
using UserModel = Vanilla.TelegramBot.Models.UserModel;

namespace Vanilla.TelegramBot.Services
{
    public class BotService : IBotService
    {
        private readonly TelegramBotClient _botClient;
        //private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;
        private readonly IBonusService _bonusService;

        private readonly Vanilla.TelegramBot.Interfaces.ILogger _logger;

        private readonly InlineSearchService _inlineSearchService;

        private List<UserContextModel> _usersContext = new List<UserContextModel>();

        public BotService(IUserService userService, IProjectService projectService, Vanilla.TelegramBot.Interfaces.ILogger logger, IBonusService bonusService, IConfiguration configuration)
        {
            _userService = userService;
            _projectService = projectService;
            _logger = logger;
            _bonusService = bonusService;

            var botToken = configuration.GetValue<string>("botAccessToken");
            var webDomainName = configuration.GetValue<string>("domain");
            var cdnDomainName = configuration.GetValue<string>("cdnDomain");

            _botClient = new TelegramBotClient(botToken);

            //var domainName = _settings.Domain;


            _inlineSearchService = new InlineSearchService(_botClient, _projectService, _userService, webDomainName, cdnDomainName);

            _logger.WriteLog("Init bot service", LogType.Information);
        }

        public async Task StartListening()
        {
            //var x = _botClient.GetWebhookInfo;

            var me = _botClient.GetMe();
            Console.WriteLine("My name is {0}.", me.FirstName);
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.WriteLine($"Start time " + DateTime.UtcNow.ToString());
            Console.WriteLine($"https://t.me/{me.Username}");

            var updates = _botClient.GetUpdates();

            if (updates.Count() > 0)
            {
                _logger.WriteLog(string.Format("Cleared {0} old messages", updates.Count()), LogType.Information);
                var clearOffset = updates.Last().UpdateId + updates.Count();
                updates = _botClient.GetUpdates(clearOffset);
            }

            while (true)
            {
                if (updates.Any())
                {
                    foreach (var update in updates)
                    {
                        // try get user context
                        var currentUserContext = GetUserContext(update);
                        //_botClient.AnswerCallbackQuery();

                        // Check is have profile
                        bool isUserHasProfile = currentUserContext.User.IsHasProfile;

                        //currentUserContext.UpdateLoadPhotoTimer();
                        if (update.Message is not null && update.Message.Photo is not null && update.Message.Photo.Count() > 0) currentUserContext.UpdateLoadPhotoTimer();

                        try
                        {
                            if (update.InlineQuery is not null)
                            {
                                InlineSearch(update, currentUserContext);
                                continue;
                            }

                            // Midlware
                            if (update.CallbackQuery is not null)
                            {
                                _botClient.AnswerCallbackQuery(update.CallbackQuery.Id);
                            }

                            if (update.Message is not null && update.Message.Chat is not null && update.Message.Chat.Type != "private" && update.Message.Chat.Id != update.Message.From.Id)
                            {
                                _logger.WriteLog("Access denied. Message originates from outside the private chat. User TG ID: " + update.Message.From.Id.ToString(), LogType.Error);
                                continue;
                            } // temp fix



                            if (BotSleshCommandHendler(update, currentUserContext)) {
                                if(currentUserContext.Folder is not null) currentUserContext.Folder.CloseFolder();
                                continue;
                            }; // temp fix

                            if (update.Message is not null && update.Message.ViaBot is not null && isUserHasProfile is false) continue; // ignore message from inline when user don`t registred

                            if (isUserHasProfile is false)
                            {
                                // Registration process
                                if (currentUserContext.Folder is not null)
                                {
                                    currentUserContext.Folder.EnterPoint(update);
                                }
                                else
                                {
                                    ToInitUser(update, currentUserContext);
                                }

                                continue;
                            }

                            if (BotInlineComandHendler(update, currentUserContext)) continue;
                            if (BotReplyComandHendler(update, currentUserContext)) continue;


                            if (currentUserContext.Folder is not null)
                            {
                                currentUserContext.Folder.EnterPoint(update);
                            }

                            if (update.Message is not null) { }
       /*                     else if (update.CallbackQuery is not null)
                            {
                                var messageText = update.CallbackQuery.Data;
                                if (messageText == "AddProject")
                                {
                                    ToAddProjects(update, currentUserContext);
                                    continue;
                                }
                                else if (messageText == "MainMenu")
                                {
                                    OpenMainMenu(currentUserContext);
                                    continue;
                                }

                            }*/
                            else if (update.PollAnswer is not null) { }
                        }
                        catch (Exception ex)
                        {
                            Guid exeptionId = _logger.WriteLog(ex.Message, LogType.Error, UserId: currentUserContext.User.UserId);

                            var mess = string.Format(currentUserContext.ResourceManager.GetString("ServerError"), "@Yumikki", exeptionId);
                            _botClient.SendMessage(chatId: currentUserContext.User.TelegramId, replyMarkup: Keyboards.GetErrorKeypoard(currentUserContext), text: mess, parseMode: "HTML");
                            
                        }
                    }

                    var offset = updates.Last().UpdateId + 1;
                    updates = _botClient.GetUpdates(offset);
                }
                else
                {
                    updates = _botClient.GetUpdates();
                }
            }


            Console.ReadLine();
        }


        private bool BotInlineComandHendler(Update update, UserContextModel userContext)
        {
            void CloseFolderIfExist()
            {
                if (userContext.Folder is not null) userContext.Folder.CloseFolder();
            }


            if (update.CallbackQuery is null) return false;

            if (Helpers.ValidatorHelpers.CallbackBtnActionValidate(update, "AddProject"))
            {
                CloseFolderIfExist();
                ToAddProjects(update, userContext);
                return true;
            }
            else if (Helpers.ValidatorHelpers.CallbackBtnActionValidate(update, "MainMenu"))
            {
                CloseFolderIfExist();
                OpenMainMenu(userContext);
                return true;
            }
            else if (Helpers.ValidatorHelpers.CallbackBtnActionValidate(update, "ReloadUserContext"))
            {
                var chatId = userContext.User.TelegramId;
                var mainMenuKeyboard = Keyboards.MainMenu(userContext);

                _usersContext.Remove(userContext);

                _botClient.SendMessage(chatId, "Lumi successfully rebooted", replyMarkup: mainMenuKeyboard);

                return true;
            }

            return false;
        }


        private string _deliver = " mya~ ";
        private bool BotReplyComandHendler(Update update, UserContextModel userContext)
        {
            if (update.Message is null) return false;
            bool isUserHaveRunTask = userContext.Folder is not null;

            var chatId = userContext.User.TelegramId; //also fix

            if (userContext.Folder is null && update.Message.Text == userContext.ResourceManager.GetString("AddProject"))
            {
                DeleteAllMesssages(userContext);
                ToAddProjects(update, userContext);
                return true;
            }
            else if (userContext.Folder is null && update.Message.Text == userContext.ResourceManager.GetString("ViewOwnProjects"))
            {
                DeleteAllMesssages(userContext);

                ToUpdateProjects(update, userContext);

                return true;
            }
            else if (userContext.Folder is null && update.Message.Text == userContext.ResourceManager.GetString("MyProfile"))
            {
                DeleteAllMesssages(userContext);
                ToInfoUserToInfoUser(update, userContext);
                return true;
            }
            else if (userContext.Folder is null && update.Message.Text == userContext.ResourceManager.GetString("BonusSytemBtn"))
            {
                DeleteAllMesssages(userContext);
                ToBonusInfoUser(update, userContext);
                return true;
            }
            else if (update.Message.Text == userContext.ResourceManager.GetString("MyProfileUpdate"))
            {
                if (userContext.Folder is not null) userContext.Folder.CloseFolder();

                DeleteAllMesssages(userContext);
                ToUpdateUser(update, userContext);
                return true;
            }
            else if (update.Message.Text == userContext.ResourceManager.GetString("Cannel"))
            {
                DeleteMessage(userContext.User.TelegramId, update.Message.MessageId);

                if (userContext.Folder is not null)
                {
                    userContext.Folder.CloseFolder();

                    OpenMainMenu(userContext);
                }
                else
                {
                    OpenMainMenu(userContext);
                }
                return true;
            }
            return false;

        }

        private void OpenMainMenu(UserContextModel userContext)
        {
            if (userContext.User.IsHasProfile == false) return;

            var message_obj = _botClient.SendMessage(userContext.User.TelegramId, userContext.ResourceManager.GetString("MainMenuSendMes"), replyMarkup: Keyboards.MainMenu(userContext));
            userContext.SendMessages.Add(message_obj.MessageId);
        }

        private void ToAddProjects(Update update, UserContextModel userContext) => ToInitFolder(update, userContext, new CreateProjectFolder(_botClient, userContext, _userService, _logger, true, _projectService));
        private void ToUpdateProjects(Update update, UserContextModel userContext) => ToInitFolder(update, userContext, new AboutProjectFolder(_botClient, userContext, _userService, _logger, true, _projectService));
        private void ToInitUser(Update update, UserContextModel userContext) => ToInitFolder(update, userContext, new UserCreateProfileFolder(_botClient, userContext, _userService, _logger));
        private void ToUpdateUser(Update update, UserContextModel userContext) => ToInitFolder(update, userContext, new UserUpdateProfileFolder(_botClient, userContext, _userService, _logger));
        private void ToInfoUserToInfoUser(Update update, UserContextModel userContext) => ToInitFolder(update, userContext, new UserGetProfileFolder(_botClient, userContext, _userService, _logger));
        private void ToBonusInfoUser(Update update, UserContextModel userContext) => ToInitFolder(update, userContext, new UserBonusFolder(_botClient, userContext, _userService, _logger, _bonusService));


        private bool BotSleshCommandHendler(Update update, UserContextModel userContext)
        {
            if (update.Message is null) return false;
            if (update.Message.Text is null) return false;
            if (update.Message.Text.First() != '/') return false;
            if (update.Message.Text.Split("/").Length != 2) return false;
            if (update.Message.Text.Split(" ").Length > 2) return false;

            bool HasProfile = userContext.User.IsHasProfile;


            if (update.Message.Text == "/menu")
            {
                DeleteMessage(userContext.User.TelegramId, update.Message.MessageId);
                DeleteAllMesssages(userContext);

                if (!HasProfile) return false;

                var messageObj = _botClient.SendMessage(update.Message.Chat.Id, userContext.ResourceManager.GetString("MainMenuSendMes"), replyMarkup: Keyboards.MainMenu(userContext));
                userContext.SendMessages.Add(messageObj.MessageId);

                return true;
            }
            else if (update.Message.Text == "/start")
            {

                // First chat message fix
                if (HasProfile)
                {
                    DeleteMessage(userContext.User.TelegramId, update.Message.MessageId);
                    DeleteAllMesssages(userContext);
                }

                var username = update.Message.Chat.FirstName ?? update.Message.Chat.Username ?? "";
                string welcomeMessage = HasProfile
                    ? string.Format(userContext.ResourceManager.GetString("Welcome"), username, _botClient.GetMe().Username)
                    : string.Format(userContext.ResourceManager.GetString("WelcomeNewUser"), username, _botClient.GetMe().Username);

                var keyboard = HasProfile ? Keyboards.InlineStartMenuKeyboard(userContext) : Keyboards.GetCreateProfileKeypoardWithSearch(userContext);
                var messageObj = _botClient.SendMessage(update.Message.Chat.Id, welcomeMessage, replyMarkup: keyboard, parseMode: "HTML");

                userContext.SendMessages.Add(messageObj.MessageId);

                //_botClient.SendMessage(update.Message.Chat.Id, welcomeMessage, replyMarkup: Keyboards.MainMenu(userContext), parseMode: "HTML");
                return true;
            }
            else if (update.Message.Text == "/start addProject")
            {
                if (!HasProfile) return true;

                //ToCreateProject(update, userContext);
                ToAddProjects(update, userContext);
                return true;
            }
            else if (update.Message.Text == "/info")
            {
                DeleteMessage(userContext.User.TelegramId, update.Message.MessageId);
                DeleteAllMesssages(userContext);

                var ms = userContext.ResourceManager.GetString("About");

                var commitDate = DateTime.Parse(ThisAssembly.Git.CommitDate).ToString("yyyy-MM-dd HH:mm");
                ms += String.Format("\n\n<i>Version: {0} {1}</i>", commitDate, ThisAssembly.Git.Commit.ToString());

                _logger.WriteLog(ms, LogType.Information);
                SendMessageArgs inputMessage = new SendMessageArgs(update.Message.Chat.Id, ms);
                inputMessage.ParseMode = "HTML";

                var messageObj = _botClient.SendMessage(inputMessage);
                userContext.SendMessages.Add(messageObj.MessageId);
                return true;

            }
            else if (update.Message.Text == "/test-exeption")
            {
                throw new Exception("test exeption");
            }
            else if (update.Message.Text == "/update")
            {
                if (!HasProfile) return true;

                ToUpdateUser(update, userContext);
                return true;
            }
            else if (update.Message.Text == "/init")
            {
                ToInitUser(update, userContext);
                return true;
            }
            else if (update.Message.Text == "/deletemyself")
            {
                DeleteMyself(userContext);
                return true;
            }
            else
            {
                var ms = userContext.ResourceManager.GetString("CommandNotRecognized");
                var formatedMs = string.Format(ms, update.Message.Text);

                SendMessageArgs inputMessage = new SendMessageArgs(update.Message.Chat.Id, formatedMs);
                inputMessage.ParseMode = "HTML";

                _botClient.SendMessage(inputMessage);
            }


            return false;
        }

        private void InlineSearch(Update update, UserContextModel userContext)
        {
            _inlineSearchService.InlineSearch(update, userContext);
        }

        private void DeleteMessage(long chatId, int messageId)
        {
            try
            {
                _botClient.DeleteMessage(chatId, messageId);
            }
            catch (Exception ex)
            {
                _logger.WriteLog(ex.Message, LogType.Error);
            }
        }

        private UserContextModel GetUserContext(Update update)
        {
            Models.UserModel user;

            long chatId = -1;

            string? username = null;
            string? firstname = null;
            string? lastname = null;
            string? languageCode = null;

            if (update.Message is not null)
            {
                chatId = update.Message.Chat.Id;

                username = update.Message.Chat.Username;
                firstname = update.Message.Chat.FirstName;
                lastname = update.Message.Chat.LastName;
            }
            if (update.Message is not null && update.Message.From is not null)
            {
                chatId = update.Message.From.Id;

                username = update.Message.From.Username;
                firstname = update.Message.From.FirstName;
                lastname = update.Message.From.LastName;
                languageCode = update.Message.From.LanguageCode;
            }
            else if (update.InlineQuery is not null)
            {
                chatId = update.InlineQuery.From.Id;

                username = update.InlineQuery.From.Username;
                firstname = update.InlineQuery.From.FirstName;
                lastname = update.InlineQuery.From.LastName;
                languageCode = update.InlineQuery.From.LanguageCode;
            }
            else if (update.PollAnswer is not null)
            {
                chatId = update.PollAnswer.User.Id;

                username = update.PollAnswer.User.Username;
                firstname = update.PollAnswer.User.FirstName;
                lastname = update.PollAnswer.User.LastName;
                languageCode = update.PollAnswer.User.LanguageCode;
            }
            else if (update.CallbackQuery is not null)
            {
                chatId = update.CallbackQuery.From.Id;

                username = update.CallbackQuery.From.Username;
                firstname = update.CallbackQuery.From.FirstName;
                lastname = update.CallbackQuery.From.LastName;
                languageCode = update.CallbackQuery.From.LanguageCode;
            }
            else if (update.ChosenInlineResult is not null)
            {
                chatId = update.ChosenInlineResult.From.Id;

                username = update.ChosenInlineResult.From.Username;
                firstname = update.ChosenInlineResult.From.FirstName;
                lastname = update.ChosenInlineResult.From.LastName;
                languageCode = update.ChosenInlineResult.From.LanguageCode;
            }
            else
            {
                _logger.WriteLog("Don`t find user tg id; For update type: " + update.GetUpdateType(), LogType.Error);
                throw new Exception("Don`t find user tg id");
            }

            UserContextModel? userContext = null;
            try
            {
                // Is user registrated in our service?
                user = _userService.SignInUser(chatId).Result;
            }
            catch
            {
                /*if (update.Message is null)
                {
                    _logger.WriteLog("The user could not be created because the action was called outside the scope of the private session.", LogType.Error);
                    //throw new Exception("Unable to create user");
                }*/

                if (chatId <= 0)
                {
                    _logger.WriteLog("The user could not be created because the action was called outside the scope of the private session.", LogType.Error);
                    throw new Exception("Unable to create user");
                }

                // Init registration in server
                user = _userService.RegisterUser(new UserRegisterModel
                {
                    TelegramId = chatId,
                    Username = username,
                    FirstName = firstname,
                    LastName = lastname,
                    LanguageCode = languageCode,
                }).Result;

                userContext = new UserContextModel(user);
                _usersContext.Add(userContext);

                string welcomeMessage = user.Username is not null || user.FirstName is not null ? string.Format(userContext.ResourceManager.GetString("WelcomeUserMessage"), userContext.User.Username) : userContext.ResourceManager.GetString("WelcomeMessage");

                try
                {
                    _botClient.SendMessage(chatId, welcomeMessage);
                }
                catch
                {
                    _logger.WriteLog("I can't send a greeting message", LogType.Warning);
                }

                _logger.WriteLog("Added new user: " + user.Username, LogType.Information, UserId: userContext.User.UserId);
            }

            if (!_usersContext.Exists(x => x.User.UserId == user.UserId)) _usersContext.Add(new UserContextModel(user));

            if(userContext is null) userContext = _usersContext.First(x => x.User.UserId == user.UserId);

            return userContext;
        }



        private void DeleteAllMesssages(UserContextModel userContext)
        {

            try
            {
                if(userContext.SendMessages.Count() > 0) _botClient.DeleteMessages(userContext.User.TelegramId, userContext.SendMessages);
            }
            catch (Exception ex)
            {
                _logger.WriteLog(ex.Message, LogType.Error, UserId: userContext.User.UserId);
            }

            userContext.SendMessages.Clear();
        }


        private void ToInitFolder(Update update, UserContextModel userContext, IFolder folder)
        {
            void destroyFolderContext()
            {
                userContext.Folder.CloseFolderEvent -= destroyFolderContext;
                userContext.Folder = null;

                OpenMainMenu(userContext);
            }

            if (userContext.Folder is null)
            {
                userContext.Folder = folder;
                userContext.Folder.CloseFolderEvent += destroyFolderContext;
                userContext.Folder.Run();
            }
            else
            {
                _logger.WriteLog("User have no closed folder context", LogType.Error, UserId: userContext.User.UserId);
                destroyFolderContext();
                ToInfoUserToInfoUser(update, userContext);
            }

            // Delete slesh message
            int messageId = update.Message is not null ? update.Message.MessageId : update.CallbackQuery.Message.MessageId;
            DeleteMessage(userContext.User.TelegramId, messageId);
        }


        // Dev func
        public void DeleteMyself(UserContextModel userContext)
        {
            _userService.DeleteUser(userContext.User.UserId);
            _usersContext.Remove(userContext);
        }


    }
}
