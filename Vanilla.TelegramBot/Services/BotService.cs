using Microsoft.Extensions.Configuration;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.Extensions;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.InlineMode;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla.Common;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Entityes;
using Vanilla.TelegramBot.Helpers;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages.Bonus;
using Vanilla.TelegramBot.Pages.CreateUser;
using Vanilla.TelegramBot.Pages.Projects;
using Vanilla.TelegramBot.Pages.UpdateUser;
using Vanilla.TelegramBot.Pages.User;
using Vanilla.TelegramBot.UI;
using Vanilla_App.Interfaces;
using Vanilla_App.Models;
using UserModel = Vanilla.TelegramBot.Models.UserModel;

namespace Vanilla.TelegramBot.Services
{

    public class BotService : IBotService
    {
        private readonly TelegramBotClient _botClient;
        private readonly SettingsModel _settings;
        //private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;
        private readonly IBonusService _bonusService;

        private readonly ILogger _logger;

        private readonly InlineSearchService _inlineSearchService;

        private List<UserContextModel> _usersContext = new List<UserContextModel>();

        public BotService(IUserService userService, IProjectService projectService, ILogger logger, IBonusService bonusService)
        {
            _userService = userService;
            _projectService = projectService;
            _logger = logger;
            _bonusService = bonusService;


            ConfigurationMeneger confManager = new ConfigurationMeneger();
            var _settings = confManager.Settings;

/*            // Build a config object, using env vars and JSON providers.
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            // Get values from the config given their key and their target type.
            _settings = config.GetRequiredSection("Settings").Get<SettingsModel>();*/
            //if (_settings == null) throw new Exception("No found setting section");

            string botToken = Environment.GetEnvironmentVariable("ACCESS_TOKEN") ?? _settings.BotAccessToken;
            _botClient = new TelegramBotClient(botToken);

            var domainName = _settings.Domain;


            _inlineSearchService = new InlineSearchService(_botClient, _projectService, _userService, domainName);

            _logger.WriteLog("Init bot service", LogType.Information);






     /*       // dev fix
            var allUsers = _userService.GetUsers().Result.Where(x => x.IsHasProfile == true && x.Images is not null && x.Images.Count() > 0);
            foreach (var user in allUsers)
            {
                LoadProfileImgIfDontExist(user);
            }*/
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
                            if (BotSleshCommandHendler(update, currentUserContext)) continue;


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


            if (update.Message.Text == "/menu")
            {
                DeleteMessage(userContext.User.TelegramId, update.Message.MessageId);
                DeleteAllMesssages(userContext);

                var messageObj = _botClient.SendMessage(update.Message.Chat.Id, userContext.ResourceManager.GetString("MainMenuSendMes"), replyMarkup: Keyboards.MainMenu(userContext));
                userContext.SendMessages.Add(messageObj.MessageId);
                return true;
            }
            else if (update.Message.Text == "/start")
            {
                var username = update.Message.Chat.FirstName ?? update.Message.Chat.Username ?? "";
                string welcomeMessage = string.Format(userContext.ResourceManager.GetString("Welcome"), username, _botClient.GetMe().Username);
                DeleteMessage(userContext.User.TelegramId, update.Message.MessageId);
                DeleteAllMesssages(userContext);
                var messageObj = _botClient.SendMessage(update.Message.Chat.Id, welcomeMessage, replyMarkup: Keyboards.InlineStartMenuKeyboard(userContext), parseMode: "HTML");
                userContext.SendMessages.Add(messageObj.MessageId);
                //_botClient.SendMessage(update.Message.Chat.Id, welcomeMessage, replyMarkup: Keyboards.MainMenu(userContext), parseMode: "HTML");
                return true;
            }
            else if (update.Message.Text == "/start addProject")
            {
                //ToCreateProject(update, userContext);
                ToAddProjects(update, userContext);
                return true;
            }
            else if (update.Message.Text == "/info")
            {
                DeleteMessage(userContext.User.TelegramId, update.Message.MessageId);
                DeleteAllMesssages(userContext);

                var ms = userContext.ResourceManager.GetString("About");
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

      /*  private void InlineSearch(Update update, UserContextModel userContext)
        {
            var inline = update.InlineQuery;
            var query = inline.Query;

            //future function
            var inlineOffset = inline.Offset;

            var inlineAddOwnProjectButton = new InlineQueryResultsButton
            {
                Text = userContext.ResourceManager.GetString("AddOwnProject"),
                StartParameter = "addProject"
            };

            var res = new List<InlineQueryResult>();

            var projects = _projectService.ProjectGetAllAsync().Result.OrderByDescending(x => x.Created).ToList();
            if (query is not null && query != "")
            {
                if (query.Contains("@"))
                {
                    var username = query.Substring(1).Split(" ")[0];
                    var users = _userService.FindByUsername(username).Result;
                    //projects = projects.Where(x => users.Where(y => y.UserId == x.OwnerId));
                    var preProjects = new List<ProjectModel>();
                    var userSerchQuery = query.Split(" ");
                    var q = userSerchQuery.Length > 1 ? string.Concat(userSerchQuery.Skip(1).ToArray()) : null;
                    foreach (var user in users)
                    {
                        var searchResult = projects.Where(x => x.OwnerId == user.UserId);
                        if (q is not null)
                        {
                            searchResult = searchResult.Where(x => x.Name.Contains(q, StringComparison.InvariantCultureIgnoreCase));
                        };
                        preProjects.AddRange(searchResult);
                    }
                    projects = preProjects;


                    if (userSerchQuery.Length > 1)
                    {

                        preProjects.Where(x => x.Name.Contains(q, StringComparison.InvariantCultureIgnoreCase)).ToList();
                    }
                }
                else
                {
                    projects = projects.Where(x => x.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase)).ToList();
                }
            }

            int maxProjects = 48;
            int index = inlineOffset is not null && inlineOffset != "" ? int.Parse(inlineOffset) + 1 : 0;
            int toIndex = index + 1 + maxProjects <= projects.Count ? index + 1 + maxProjects : projects.Count;

            var offsetProjectId = toIndex < projects.Count ? toIndex.ToString() : null;


            while (index < toIndex)
            {
                //var project in projects
                var project = projects[index];

                //var messageContent = AboutProjectFormating(project);
                var owner = _userService.GetUser(project.OwnerId).Result;
                var messageContent = MessageWidgets.AboutProject(project, owner, userContext);

                var inputMessage = new InputTextMessageContent(messageContent);
                inputMessage.ParseMode = "HTML";

                string developStatusEmoji = FormationHelper.GetEmojiStatus(project.DevelopmentStatus);

                var ownerName = owner.Username is not null ? "@" + owner.Username : owner.FirstName;
                var desription = developStatusEmoji + " " + ownerName + "\n" + project.Description;

                int messageMaxLenght = 4090;
                if (desription.Length >= messageMaxLenght)
                {
                    int howMuchMore = desription.Length - messageMaxLenght;
                    int abbreviation = desription.Length - howMuchMore - 3;
                    desription = ownerName + "\n" + project.Description.Substring(0, abbreviation) + "...";
                }

                //var replyMarkuppp = userContext.User.TelegramId == owner.TelegramId && inline.From.Id == userContext.User.TelegramId && inline.ChatType == "sender" ? GetProjectItemMenu(project) : null;

                res.Add(new InlineQueryResultArticle
                {
                    Id = index.ToString(),
                    Title = project.Name,
                    Description = desription,
                    //InputMessageContent = new InputTextMessageContent("project " + project.Id.ToString()),
                    InputMessageContent = inputMessage,
                    //ReplyMarkup = replyMarkuppp
                });
                index++;
            }

            var ans = new AnswerInlineQueryArgs(inline.Id, res);

            _botClient.AnswerInlineQuery(inlineQueryId: inline.Id, results: res, button: inlineAddOwnProjectButton, nextOffset: offsetProjectId, cacheTime: 24);
        }*/

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
                _logger.WriteLog("Don`t find user tg id; Update type: " + update.GetUpdateType(), LogType.Error);
                throw new Exception("Don`t find user tg id");
            }

            UserContextModel? userContext = null;
            try
            {
                user = _userService.SignInUser(chatId).Result;
            }
            catch
            {
                if (update.Message is null && update.InlineQuery is null && update.CallbackQuery is null)
                {
                    _logger.WriteLog("Unable to create user", LogType.Error);
                    throw new Exception("Unable to create user");
                }

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

            userContext = _usersContext.First(x => x.User.UserId == user.UserId);

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

        // Dev temp fix
        void LoadProfileImgIfDontExist(UserModel userModel)
        {
            var userProfileImages = userModel.Images;
            foreach (ImageModel image in userProfileImages)
            {
                if (IsProfileImageBeLoad(image.TgMediaId) is false) LoadProfileImages(userModel);
            }
        }
        bool IsProfileImageBeLoad(string TgMediaId)
        {
            var originalImagePath = "storage\\" + TgMediaId + ".jpg";
            var thumbnailImagePath = "storage\\" + TgMediaId + "_thumbnail.jpg";

            if (File.Exists(originalImagePath) is false) return false;
            if (File.Exists(thumbnailImagePath) is false) return false;

            return true;
        }

        void LoadProfileImages(UserModel userModel)
        {
            var update = new Models.UserUpdateRequestModel() {
                Images = new List<ImageModel>(),
                IsHasProfile = true,
            };

            foreach (var image in userModel.Images) {
                var file = _botClient.GetFile(image.TgMediaId);
                var imageUrl = _botClient.BuildFileDownloadLink(file);
                update.Images.Add(new ImageModel { TgMediaId = image.TgMediaId, DownloadPath = imageUrl });
            }

            _userService.UpdateUser(userModel.TelegramId, update);
        }


    }
}
