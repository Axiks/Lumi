using System.Data;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.Extensions;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages.Bonus;
using Vanilla.TelegramBot.Pages.CreateUser;
using Vanilla.TelegramBot.Pages.Projects;
using Vanilla.TelegramBot.Pages.UpdateUser;
using Vanilla.TelegramBot.Pages.User;
using Vanilla.TelegramBot.Services.Bot_Service;
using Vanilla.TelegramBot.UI;
using Vanilla.TelegramBot.UI.Widgets;
using Vanilla_App.Services.Bonus;
using Vanilla_App.Services.Projects;
using Update = Telegram.BotAPI.GettingUpdates.Update;

namespace Vanilla.TelegramBot.Services
{
    public class BotService : IBotService
    {
        private readonly TelegramBotClient _botClient;
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;
        private readonly IBonusService _bonusService;

        private readonly Vanilla.TelegramBot.Interfaces.ILogger _logger;
        private readonly ILogger<BotService> _systemLogger;

        private readonly InlineSearchModule _inlineSearchModule;

        UserContextMenager _userContextMenager;

        BotInitData _botInitInfo;


        public BotService(IUserService userService, IProjectService projectService, Vanilla.TelegramBot.Interfaces.ILogger logger, IBonusService bonusService, IConfiguration configuration, ILogger<BotService> systemLogger)
        {
            _userService = userService;
            _projectService = projectService;
            _logger = logger;
            _bonusService = bonusService;

            var botToken = configuration.GetValue<string>("botAccessToken");
            var webDomainName = configuration.GetValue<string>("domain");
            var cdnDomainName = configuration.GetValue<string>("cdnDomain");


            _botClient = new TelegramBotClient(botToken);

            try
            {
                var me = _botClient.GetMe();

                _botInitInfo = new BotInitData
                {
                    BotId = me.Id,
                    FirstName = me.FirstName,
                    Username = me.Username,
                    Administrations = new List<long> { configuration.GetValue<long>("botAdminTgId") },
                    Environment = configuration.GetValue<string>("DOTNET_ENVIRONMENT"),
                    SiteUrl = webDomainName,
                };
            }
            catch (System.Net.Http.HttpRequestException networkEx)
            {
                systemLogger.LogError("TG bot network error. {0}", networkEx);
                throw networkEx;
            }
            catch (Exception ex) {
                systemLogger.LogError("TG bot error. {0}", ex);
                throw ex;
            }
            

            _userContextMenager = new UserContextMenager();
            _inlineSearchModule = new InlineSearchModule(_botClient, _projectService, _userService, webDomainName, cdnDomainName);

            _logger.WriteLog("Init bot service", LogType.Information);
            systemLogger.LogInformation("Init bot service at {DT}", DateTime.UtcNow.ToLongTimeString());

            _systemLogger = systemLogger;
        }

        public async Task StartListening()
        {
            PrintInitBotInformation(_botInitInfo);

            var updates = _botClient.GetUpdates();
            if (updates.Count() > 0)
            {
                _logger.WriteLog(string.Format("Cleared {0} old messages", updates.Count()), LogType.Information);
                ClearLastMessages(updates);
            }

            while (true)
            {
                if (updates.Any())
                {
                    foreach (var update in updates)
                    {
                        // Pre Midleware
                        if (update.Message is not null && update.Message.Chat is not null && update.Message.Chat.Type != "private" && update.Message.Chat.Id != update.Message.From.Id)
                        {
                            _logger.WriteLog("Access denied. Message originates from outside the private chat. User TG ID: " + update.Message.From.Id.ToString(), LogType.Warning);
                            continue;
                        } // temp fix

                        // Skip if isn`t user message
                        var updateUserData = TryGetUserData(update);
                        if (updateUserData is null)
                        {
                            _logger.WriteLog("Don`t find user data for update type: " + update.GetUpdateType(), LogType.Error);

                            DiagnosticsConfig.RequestCounter.Add(
                                1,
                                new KeyValuePair<string, object?>("TelegramBot.core.request.date", DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss.fffZ"))
                            );

                            continue;
                        }


                        //If user don`t have context -> create
                        if (_userContextMenager.Get(updateUserData.TgId) is null)
                        {
                            try
                            {
                                // Add user data, from app, to user context
                                UserModel? userModel = TryGetUserContext(updateUserData);
                                _userContextMenager.Add(updateUserData, userModel);
                            }
                            catch
                            {
                                // User witchout user data in app
                                _logger.WriteLog("Catch exeption; MAYBE Don`t find user; Start basic registration; User TG ID: " + updateUserData.TgId, LogType.Warning);
                                _userContextMenager.Add(updateUserData);
                            }
                        }

                        // Get user context
                        UserContextModel currentUserContext = _userContextMenager.Get(updateUserData.TgId);
                        //if (currentUserContext.User is null) currentUserContext = _userContextMenager.Get(updateUserData.TgId); //reload context after register


                        DiagnosticsConfig.RequestCounter.Add(
                            1,
                            new KeyValuePair<string, object?>("TelegramBot.core.request.count.user.tgId", currentUserContext.UpdateUser.TgId),
                            new KeyValuePair<string, object?>("TelegramBot.core.request.count.user.Role", string.Join(",", currentUserContext.Roles)),
                            new KeyValuePair<string, object?>("TelegramBot.core.request.count.user.Id", currentUserContext.User is not null? currentUserContext.User.UserId : "null"),
                            new KeyValuePair<string, object?>("TelegramBot.core.request.date", DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss.fffZ"))
                        );

                        new BotMiddleware(_botClient, update, currentUserContext);

                        BotRouter(update, currentUserContext);

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

        void BotRouter(Update update, UserContextModel userContext)
        {
            if (update.InlineQuery is not null)
            {
                DiagnosticsConfig.SearchRequestCounter.Add(
                            1,
                            new KeyValuePair<string, object?>("TelegramBot.search.request.count.user.tgId", userContext.UpdateUser.TgId),
                            new KeyValuePair<string, object?>("TelegramBot.search.request.count.user.Role", string.Join(",", userContext.Roles)),
                            new KeyValuePair<string, object?>("TelegramBot.search.request.count.user.Id", userContext.User is not null ? userContext.User.UserId : "null"),
                            new KeyValuePair<string, object?>("TelegramBot.search.request.date", DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss.fffZ"))
                        );

                _inlineSearchModule.InlineSearch(update, userContext);
                return;
            }

            if (IsSlashCommandHelper(update))
            {
                if (userContext.Folder is not null) userContext.Folder.CloseFolder();

                BotSleshCommandHendler(update, userContext);
                return;
            };

            //if (update.Message is not null && update.Message.ViaBot is not null && userContext.IsHasProfile is false) return; // ignore message from inline when user don`t registred; temp

            if (BotInlineComandHendler(update, userContext)) return;
            if (BotReplyComandHendler(update, userContext)) return;

            if (userContext.Folder is not null)
            {
                userContext.Folder.EnterPoint(update);
                return;
            }

            if(GuardHelper(userContext, RoleEnum.Anonim)) {
                /* You need to register. */

                _botClient.SendMessage(chatId: userContext.UpdateUser.TgId, text: "You need to register.", parseMode: "HTML");
                return;
            }


            // Action don`t recognized
            Guid exeptionId = _logger.WriteLog("Unrecognized action", LogType.Warning, UserTgId: userContext.UpdateUser.TgId);
            var mess = string.Format("Unrecognized action", "@" + userContext.UpdateUser.Username, exeptionId);
            _botClient.SendMessage(chatId: userContext.UpdateUser.TgId, replyMarkup: Keyboards.GetErrorKeypoard(userContext), text: mess, parseMode: "HTML");

            return;

        }


        private bool BotInlineComandHendler(Update update, UserContextModel userContext)
        {
            void CloseFolderIfExist()
            {
                if (userContext.Folder is not null) userContext.Folder.CloseFolder();
            }

            void SendOnlyForRegisterUserMessage()
            {
                var messageObj = _botClient.SendMessage(Widjets.ThisFeatureIsOnlyForRegisteredUsers(userContext.UpdateUser.TgId, userContext.ResourceManager));
                userContext.MessageMenager.Add(messageObj.MessageId);
            }

            _systemLogger.LogDebug("Enter to BotInlineComandHendler");

            if (update.CallbackQuery is null) return false;

            if (Helpers.ValidatorHelpers.CallbackBtnActionValidate(update, "AddProject"))
            {
                if (GuardHelper(userContext, RoleEnum.Anonim))
                {
                    SendOnlyForRegisterUserMessage();
                    return false;
                }

                CloseFolderIfExist();
                ToAddProjects(update, userContext);
                return true;
            }
            else if (Helpers.ValidatorHelpers.CallbackBtnActionValidate(update, "MainMenu"))
            {
                if (GuardHelper(userContext, RoleEnum.Anonim))
                {
                    SendOnlyForRegisterUserMessage();
                    return false;
                }

                CloseFolderIfExist();
                OpenMainMenu(userContext);
                return true;
            }
            else if (Helpers.ValidatorHelpers.CallbackBtnActionValidate(update, "ReloadUserContext"))
            {
                //_usersContext.Remove(userContext);
                CloseFolderIfExist();
                _userContextMenager.Remove(userContext.UpdateUser.TgId);

                var chatId = userContext.UpdateUser.TgId;
                var mainMenuKeyboard = Keyboards.MainMenu(userContext);

                //if (userContext.IsHasProfile is true) _botClient.SendMessage(chatId, "Lumi successfully rebooted", replyMarkup: mainMenuKeyboard);
                //else _botClient.SendMessage(chatId, "Lumi successfully rebooted");

                _botClient.SendMessage(chatId, "Lumi successfully rebooted");

                return true;
            }
            else if (Helpers.ValidatorHelpers.CallbackBtnActionValidate(update, "CreateProfile"))
            {
                if (GuardHelper(userContext, RoleEnum.User))
                {
                    var message = _botClient.SendMessage(userContext.UpdateUser.TgId, "Denied. User is already registered");
                    userContext.MessageMenager.Add(message.MessageId);
                    return true;
                }

                ToInitUser(update, userContext);

                return true;
            }

            return false;
        }

        private bool BotReplyComandHendler(Update update, UserContextModel userContext)
        {
            if (update.Message is null) return false;

            _systemLogger.LogDebug("Enter to BotReplyComandHendler");


            if (update.Message.Text == userContext.ResourceManager.GetString("Cannel"))
            {
                DeleteMessage(userContext.UpdateUser.TgId, update.Message.MessageId);

                if (userContext.Folder is not null)
                {
                    userContext.Folder.CloseFolder();

                    if (GuardHelper(userContext, RoleEnum.User)) OpenMainMenu(userContext);
                }
                else
                {
                    if (GuardHelper(userContext, RoleEnum.User)) OpenMainMenu(userContext);
                }
                return true;
            }

            //if (userContext.IsHasProfile is false) return false;
            if(GuardHelper(userContext, RoleEnum.Anonim)) return false;

            //var chatId = userContext.UpdateUser.TgId; //also fix

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
            //else if (update.Message.Text == userContext.ResourceManager.GetString("MyProfileUpdate"))
            else if (update.Message.Text == userContext.ResourceManager.GetString("MyProfileUpdate"))
            {
                if (userContext.Folder is not null) userContext.Folder.CloseFolder(); // need fix in the future

                DeleteAllMesssages(userContext);
                ToUpdateUser(update, userContext);
                return true;
            }

            return false;

        }

        private void OpenMainMenu(UserContextModel userContext)
        {
            //if (userContext.IsHasProfile == false) return;
            //if(GuardHelper(userContext, RoleEmum.Anonim)) return;

            var message_obj = _botClient.SendMessage(userContext.UpdateUser.TgId, userContext.ResourceManager.GetString("MainMenuSendMes"), replyMarkup: Keyboards.MainMenu(userContext));
            userContext.MessageMenager.Add(message_obj.MessageId);
            //userContext.SendMessages.Add(message_obj.MessageId);
        }

        private void ToAddProjects(Update update, UserContextModel userContext) => ToInitFolder(update, userContext, new CreateProjectFolder(_botClient, userContext, _userService, _logger, true, _projectService));
        private void ToUpdateProjects(Update update, UserContextModel userContext) => ToInitFolder(update, userContext, new AboutProjectFolder(_botClient, userContext, _userService, _logger, true, _projectService));
        private void ToInitUser(Update update, UserContextModel userContext) => ToInitFolder(update, userContext, new UserCreateProfileFolder(_botClient, userContext, _userService, _logger), isAccessForAnonimUser: true);
        private void ToUpdateUser(Update update, UserContextModel userContext) => ToInitFolder(update, userContext, new UserUpdateProfileFolder(_botClient, userContext, _userService, _logger));
        private void ToInfoUserToInfoUser(Update update, UserContextModel userContext) => ToInitFolder(update, userContext, new UserGetProfileFolder(_botClient, userContext, _userService, _logger));
        private void ToBonusInfoUser(Update update, UserContextModel userContext) => ToInitFolder(update, userContext, new UserBonusFolder(_botClient, userContext, _userService, _logger, _bonusService));


        bool IsSlashCommandHelper(Update update)
        {
            if (update.Message is null) return false;
            if (update.Message.Text is null) return false;
            if (update.Message.Text.First() != '/') return false;
            if (update.Message.Text.Split("/").Length != 2) return false;
            if (update.Message.Text.Split(" ").Length > 2) return false;

            return true;
        }

        private bool BotSleshCommandHendler(Update update, UserContextModel userContext)
        {
            //bool HasProfile = userContext.IsHasProfile;
            bool HasProfile = userContext.Roles.Contains(RoleEnum.User);
            bool IsAdmin = _botInitInfo.Administrations.Contains(userContext.UpdateUser.TgId);

            void SendOnlyForRegisterUserMessage() {
                var messageObj = _botClient.SendMessage(Widjets.ThisFeatureIsOnlyForRegisteredUsers(userContext.UpdateUser.TgId, userContext.ResourceManager));
                userContext.MessageMenager.Add(messageObj.MessageId);
                //userContext.SendMessages.Add(messageObj.MessageId);
            }

            _systemLogger.LogDebug("Enter to BotSleshCommandHendler");


            if (update.Message.Text == "/menu")
            {
                DeleteMessage(userContext.UpdateUser.TgId, update.Message.MessageId);
                //DeleteAllMesssages(userContext);

                if (HasProfile is false)
                {
                    SendOnlyForRegisterUserMessage();
                    return false;
                }

                var messageObj = _botClient.SendMessage(update.Message.Chat.Id, userContext.ResourceManager.GetString("MainMenuSendMes"), replyMarkup: Keyboards.MainMenu(userContext));
                //userContext.SendMessages.Add(messageObj.MessageId);
                userContext.MessageMenager.Add(messageObj.MessageId);


                return true;
            }
            else if (update.Message.Text == "/start")
            {

                // First chat message fix
                if (HasProfile)
                {
                    DeleteMessage(userContext.UpdateUser.TgId, update.Message.MessageId);
                    //DeleteAllMesssages(userContext);
                }

                var username = update.Message.Chat.FirstName ?? update.Message.Chat.Username ?? "";
                string welcomeMessage = HasProfile
                    ? string.Format(userContext.ResourceManager.GetString("Welcome"), username, _botClient.GetMe().Username)
                    : string.Format(userContext.ResourceManager.GetString("WelcomeNewUser"), username, _botClient.GetMe().Username);

                var keyboard = HasProfile ? Keyboards.InlineStartMenuKeyboard(userContext) : Keyboards.GetCreateProfileKeypoardWithSearch(userContext);
                var messageObj = _botClient.SendMessage(update.Message.Chat.Id, welcomeMessage, replyMarkup: keyboard, parseMode: "HTML");
                userContext.MessageMenager.Add(messageObj.MessageId);

                return true;
            }
            else if (update.Message.Text == "/start addProject")
            {
                if (!HasProfile)
                {
                    SendOnlyForRegisterUserMessage();
                    return true;
                }

                ToAddProjects(update, userContext);
                return true;
            }
            else if (update.Message.Text == "/info")
            {
                DeleteMessage(userContext.UpdateUser.TgId, update.Message.MessageId);
                DeleteAllMesssages(userContext);

                var ms = String.Format(userContext.ResourceManager.GetString("About"), _botInitInfo.SiteUrl, new Uri(_botInitInfo.SiteUrl).Host);

                var commitDate = DateTime.Parse(ThisAssembly.Git.CommitDate).ToString("yyyy-MM-dd HH:mm");
                ms += String.Format("\n\n<i>Version: {0} {1}</i>", commitDate, ThisAssembly.Git.Commit.ToString());
                if (IsAdmin is true) ms += String.Format("\n<i>Environment: {0}</i>", _botInitInfo.Environment);

                _logger.WriteLog(ms, LogType.Information);
                SendMessageArgs inputMessage = new SendMessageArgs(update.Message.Chat.Id, ms);
                inputMessage.ParseMode = "HTML";

                var messageObj = _botClient.SendMessage(inputMessage);
                //userContext.SendMessages.Add(messageObj.MessageId);
                userContext.MessageMenager.Add(messageObj.MessageId);

                return true;

            }
            else if (IsAdmin && update.Message.Text == "/admin")
            {
                var adminMessage = "Admin menu\n\n1. /test-exeption\n2. /init\n3. /deletemyself\n 4. /delete @nickname";
                var messageObj = _botClient.SendMessage(userContext.UpdateUser.TgId, adminMessage);
                return true;
            }
            else if (IsAdmin && update.Message.Text == "/test-exeption")
            {
                throw new Exception("test exeption");
            }
            else if (IsAdmin && update.Message.Text == "/init")
            {
                ToInitUser(update, userContext);
                return true;
            }
            else if (IsAdmin && update.Message.Text == "/deletemyself")
            {
                var success = DeleteMyself(userContext).Result;
                var deleteMessage = success ? "User delete success!" : "Error";
                var messageObj = _botClient.SendMessage(userContext.UpdateUser.TgId, deleteMessage);

                return true;
            }
            else if (IsAdmin && update.Message.Text.Contains("/delete"))
            {
                if (update.Message.Text.Contains("@") is false) return false;
                string username = update.Message.Text.Split(" ").Last();

                var mwssage = "";
                if (DeleteUser(username)) mwssage = "Success";
                else mwssage = "Error";
                SendMessageArgs inputMessage = new SendMessageArgs(update.Message.Chat.Id, mwssage);
                var messageObj = _botClient.SendMessage(inputMessage);
                userContext.MessageMenager.Add(messageObj.MessageId);

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


        UpdateUserData? TryGetUserData(Update update)
        {
            //if (update.Message is not null)
            //{
            //    return new UpdateUserData {
            //        TgId = update.Message.Chat.Id,
            //        Username = update.Message.Chat.Username,
            //        Firstname = update.Message.Chat.FirstName,
            //        Lastname = update.Message.Chat.LastName,
            //    };
            //}

            UpdateUserData? updateUserData = null;

            if (update.Message is not null && update.Message.From is not null)
            {
                updateUserData = new UpdateUserData
                {
                    TgId = update.Message.From.Id,
                    Username = update.Message.From.Username,
                    Firstname = update.Message.From.FirstName,
                    Lastname = update.Message.From.LastName,
                    LanguageCode = update.Message.From.LanguageCode,
                };
            }
            else if (update.InlineQuery is not null)
            {
                updateUserData = new UpdateUserData
                {
                    TgId = update.InlineQuery.From.Id,
                    Username = update.InlineQuery.From.Username,
                    Firstname = update.InlineQuery.From.FirstName,
                    Lastname = update.InlineQuery.From.LastName,
                    LanguageCode = update.InlineQuery.From.LanguageCode,
                };
            }
            else if (update.PollAnswer is not null)
            {
                updateUserData = new UpdateUserData
                {
                    TgId = update.PollAnswer.User.Id,
                    Username = update.PollAnswer.User.Username,
                    Firstname = update.PollAnswer.User.FirstName,
                    Lastname = update.PollAnswer.User.LastName,
                    LanguageCode = update.PollAnswer.User.LanguageCode,
                };
            }
            else if (update.CallbackQuery is not null)
            {
                updateUserData = new UpdateUserData
                {
                    TgId = update.CallbackQuery.From.Id,
                    Username = update.CallbackQuery.From.Username,
                    Firstname = update.CallbackQuery.From.FirstName,
                    Lastname = update.CallbackQuery.From.LastName,
                    LanguageCode = update.CallbackQuery.From.LanguageCode,
                };
            }
            else if (update.ChosenInlineResult is not null)
            {
                updateUserData = new UpdateUserData
                {
                    TgId = update.ChosenInlineResult.From.Id,
                    Username = update.ChosenInlineResult.From.Username,
                    Firstname = update.ChosenInlineResult.From.FirstName,
                    Lastname = update.ChosenInlineResult.From.LastName,
                    LanguageCode = update.ChosenInlineResult.From.LanguageCode,
                };
            }

            if (updateUserData is not null && updateUserData.TgId <= 0)
            {
                _logger.WriteLog("Unable to retrieve user data because the message was not sent by the user.", LogType.Error);
                //throw new Exception("Unable to create user");
            }

            if(_botInitInfo.Administrations.Contains(updateUserData.TgId)) updateUserData.IsAdmin = true;

            return updateUserData;
        }

        UserModel? TryGetUserContext(UpdateUserData userData)
        {
            var user = _userService.SignInUserAsync(userData.TgId).Result;
            return user;
        }


        private void DeleteAllMesssages(UserContextModel userContext)
        {
            try
            {
                if (userContext.MessageMenager.SendMessages.Count() > 0) _botClient.DeleteMessages(userContext.UpdateUser.TgId, userContext.MessageMenager.SendMessages);
            }
            catch (Exception ex)
            {
                _logger.WriteLog(ex.Message, LogType.Error, UserId: userContext.User.UserId);
            }

            userContext.MessageMenager.Clear();
        }


        private void ToInitFolder(Update update, UserContextModel userContext, IFolder folder, bool isAccessForAnonimUser = false)
        {
            void destroyFolderContext()
            {
                userContext.Folder.CloseFolderEvent -= destroyFolderContext;
                userContext.Folder = null;

                OpenMainMenu(userContext);
            }

            if (userContext.Roles.Contains(RoleEnum.User) is false && isAccessForAnonimUser is false)
            {
                var messageObj = _botClient.SendMessage(Widjets.ThisFeatureIsOnlyForRegisteredUsers(userContext.UpdateUser.TgId, userContext.ResourceManager));
                userContext.MessageMenager.Add(messageObj.MessageId);
                return;
            }

            if (userContext.Folder is null)
            {
                userContext.Folder = folder;
                userContext.Folder.CloseFolderEvent += destroyFolderContext;
                userContext.Folder.Run();
            }
            else
            {
                _logger.WriteLog("User have no closed folder context", LogType.Error, UserTgId: userContext.UpdateUser.TgId);
                destroyFolderContext();
                //ToInfoUserToInfoUser(update, userContext); //fix?
            }

            // Delete slesh message
            int messageId = update.Message is not null ? update.Message.MessageId : update.CallbackQuery.Message.MessageId;
            DeleteMessage(userContext.UpdateUser.TgId, messageId);
        }

        void PrintInitBotInformation(BotInitData bot)
        {
            Console.WriteLine("My name is {0}.", bot.FirstName);
            Console.WriteLine($"Start listening for @{bot.Username}");
            Console.WriteLine($"Start time " + DateTime.UtcNow.ToString());
            Console.WriteLine($"https://t.me/{bot.Username}");
        }


        void ClearLastMessages(IEnumerable<Telegram.BotAPI.GettingUpdates.Update> updates)
        {
            var clearOffset = updates.Last().UpdateId + updates.Count();
            updates = _botClient.GetUpdates(clearOffset);
        }


        // Dev func
        public bool DeleteUser(string username)
        {
            var user = _userService.FindByUsernameAsync(username).Result.FirstOrDefault();
            if (user != null) return false;
            _userService.DeleteUserAsync(user.UserId);
            //_usersContext.Remove(userContext);
            _userContextMenager.Remove(user.TelegramId);

            return true;
        }
        public async Task<bool> DeleteMyself(UserContextModel userContext)
        {
            try
            {
                await _userService.DeleteUserAsync(userContext.User.UserId);

                _userContextMenager.Remove(userContext.User.UserId);
                return true;
            }
            catch (Exception ex) {
                return false;

            }
        }

        bool GuardHelper(UserContextModel userContext, RoleEnum role)
        {
            if(userContext.Roles.Contains(role)) return true;

            _systemLogger.LogWarning("Access denied for: {USERTGID} {ROLELIST}; Expected:", userContext.UpdateUser.TgId, userContext.Roles.ToString(), role.ToString());
            return false;
        }


    }
}
