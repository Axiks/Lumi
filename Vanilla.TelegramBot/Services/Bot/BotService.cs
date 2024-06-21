using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.Extensions;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.InlineMode;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Entityes;
using Vanilla.TelegramBot.Helpers;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.UI;
using Vanilla_App.Interfaces;
using Vanilla_App.Models;

namespace Vanilla.TelegramBot.Services.Bot
{
    public class BotService : IBotService
    {
        private readonly TelegramBotClient _botClient;
        private readonly SettingsModel _settings;
        //private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;

        private readonly ILogger _logger;

        //private readonly string[] mainMenuitems = { "Add project", "View own projects" };

        private List<UserContextModel> _usersContext = new List<UserContextModel>();

        private readonly List<string> _punktsMenu = new List<string>
                            {
                                "update name",
                                "update description",
                                "update status",
                                "update links",
                            };
        public BotService(IUserService userService, IProjectService projectService, ILogger logger)
        {
            _userService = userService;
            _projectService = projectService;
            _logger = logger;

            // Build a config object, using env vars and JSON providers.
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            // Get values from the config given their key and their target type.
            _settings = config.GetRequiredSection("Settings").Get<SettingsModel>();
            if (_settings == null) throw new Exception("No found setting section");

            string botToken = Environment.GetEnvironmentVariable("ACCESS_TOKEN") ?? _settings.BotAccessToken;
            _botClient = new TelegramBotClient(botToken);

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
                _logger.WriteLog(String.Format("Cleared {0} old messages", updates.Count()), LogType.Information);
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

                            if (BotInlineComandHendler(update, currentUserContext)) continue;
                            if (BotSleshCommandHendler(update, currentUserContext)) continue;


                            if (currentUserContext.BotProjectCreator is not null)
                            {
                                currentUserContext.BotProjectCreator.EnterPoint(update);
                                continue;
                            }
                            else if (currentUserContext.BotProjectUpdater is not null)
                            {
                                currentUserContext.BotProjectUpdater.EnterPoint(update);
                                continue;
                            }

                            if (update.Message is not null) { }
                            else if (update.CallbackQuery is not null)
                            {
                                var messageText = update.CallbackQuery.Data;
                                if(messageText == "AddProject")
                                {
                                    ToCreateProject(update, currentUserContext);
                                    continue;
                                }
                                else if (messageText == "MainMenu")
                                {
                                    _botClient.SendMessage(currentUserContext.User.TelegramId, currentUserContext.ResourceManager.GetString("MainMenu"), replyMarkup: Keyboards.MainMenu(currentUserContext));
                                    continue;
                                }
                                var userContext = GetUserContext(update);
                                ToUpdateProject(update, userContext);

                            }
                            else if (update.PollAnswer is not null) { }
                        }
                        catch (Exception ex)
                        {
                            Guid exeptionId = Guid.NewGuid();
                            var mess = String.Format(currentUserContext.ResourceManager.GetString("ServerError"), "@Yumikki", exeptionId);
                            _botClient.SendMessage(chatId: currentUserContext.User.TelegramId, mess, parseMode: "HTML");
                            _logger.WriteLog(ex.Message + "; Exeption ID: " + exeptionId, LogType.Error);
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


        private string _deliver = " mya~ ";
        private bool BotInlineComandHendler(Telegram.BotAPI.GettingUpdates.Update update, UserContextModel userContext)
        {
            if (update.Message is null) return false;
            bool isUserHaveRunTask = userContext.BotProjectCreator is not null || userContext.UpdateProjectContext is not null;

            var chatId = userContext.User.TelegramId; //also fix

            if (!isUserHaveRunTask && update.Message.Text == userContext.ResourceManager.GetString("AddProject"))
            {
                //DeleteMessage(userContext.User.TelegramId, update.Message.MessageId);
                ToCreateProject(update, userContext);
                return true;
            }
            else if (!isUserHaveRunTask && update.Message.Text == userContext.ResourceManager.GetString("ViewOwnProjects"))
            {
                //DeleteMessage(userContext.User.TelegramId, update.Message.MessageId);
                ToViewUserProjets(update, userContext);
                return true;
            }
            else if (update.Message.Text == userContext.ResourceManager.GetString("Cannel"))
            {
                DeleteMessage(userContext.User.TelegramId, update.Message.MessageId);

                if (userContext.BotProjectCreator is not null)
                {
                    // Reset create project
                    //_botClient.DeleteMessages(chatId: chatId, messageIds: userContext.CreateProjectContext.SendedMessages);
                    userContext.BotProjectCreator.ClearMessages();
                    userContext.CreateProjectContext = null;
                    userContext.BotProjectCreator = null;

                    _botClient.SendMessage(update.Message.Chat.Id, userContext.ResourceManager.GetString("MainMenu"), replyMarkup: Keyboards.MainMenu(userContext));
                }
                else if (userContext.BotProjectUpdater is not null)
                {
                    // Reset update project
                    userContext.BotProjectUpdater.ClearMessages();
                    userContext.BotProjectUpdater = null;

                    _botClient.SendMessage(update.Message.Chat.Id, userContext.ResourceManager.GetString("MainMenu"), replyMarkup: Keyboards.MainMenu(userContext));
                }
                else
                {
                    _botClient.SendMessage(update.Message.Chat.Id, userContext.ResourceManager.GetString("MainMenu"), replyMarkup: Keyboards.MainMenu(userContext));
                }
                return true;
            }
            return false;
            
        }
        public void OnCreated(UserContextModel userContext)
        {
            userContext.CreateProjectContext = null;
            userContext.BotProjectCreator = null;
        }
        public void OnUpdated(UserContextModel userContext)
        {
            var messageToUpdate = userContext.BotProjectUpdater.updateMessageId;

            var projectId = userContext.BotProjectUpdater.projectModel.Id;
            var project = _projectService.ProjectGetAsync(projectId).Result;
            var message = MessageWidgets.AboutProject(project, userContext.User, userContext);
            var replyMarkuppp = GetProjectItemMenu(project, userContext);
            _botClient.EditMessageText(chatId: userContext.User.TelegramId, messageId: messageToUpdate, text: message, replyMarkup: replyMarkuppp, parseMode: "HTML");

            userContext.UpdateProjectContext = null;
            userContext.BotProjectUpdater = null;
        }

        private void ToViewUserProjets(Telegram.BotAPI.GettingUpdates.Update update, UserContextModel userContext)
        {
            var chatId = update.Message.Chat.Id;
            // temp fix
            var userProjects = _projectService.ProjectGetAllAsync().Result.Where(x => x.OwnerId == userContext.User.UserId).OrderBy(x => x.Created);

            var makeNewProjectBtn = new InlineKeyboardButton(text: "Add project");
            makeNewProjectBtn.CallbackData = userContext.ResourceManager.GetString("AddProject");

            var replyNoProjectMarkup = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                                            new InlineKeyboardButton[]{
                                                makeNewProjectBtn
                                            }
                }
            );

            if (userProjects == null || userProjects.Count() == 0) _botClient.SendMessage(chatId, userContext.ResourceManager.GetString("UserDontHaveProjectsMess"), replyMarkup: replyNoProjectMarkup);


            foreach (var project in userProjects)
            {
                string deliver = " mya~ ";

                var replyMarkuppp = GetProjectItemMenu(project, userContext);

                _botClient.SendMessage(chatId, MessageWidgets.AboutProject(project, userContext.User, userContext),
                    replyMarkup: replyMarkuppp, parseMode: "HTML");
            }

            try
            {
                _botClient.DeleteMessage(userContext.User.TelegramId, update.Message.MessageId);
            }
            catch (Exception ex)
            {
                _logger.WriteLog(ex.Message, LogType.Error);
            }
        }

        private void ToUpdateProject(Telegram.BotAPI.GettingUpdates.Update update, UserContextModel userContext)
        {
            var message = update.CallbackQuery.Data;
            var x = message.Split(_deliver).First().Split(" ");
            var y = message.Split(_deliver).Length == 2;
            var z = message.Split(_deliver).First().Split(" ").First() == "update";

            if (message.Split(_deliver).Length == 2 && message.Split(_deliver).First() == "update" && message.Split(_deliver).First().Split(" ").Length == 1)
            {

                var projectId = Guid.Parse(message.Split(_deliver).Last());
                var projectModel = _projectService.ProjectGetAsync(projectId).Result;
                ViewUpdateMenu(update, userContext, projectModel);
            }
            else if (message.Split(_deliver).Length == 2 && message.Split(_deliver).First().Split(" ").First() == "update" && message.Split(_deliver).First().Split(" ").Length == 2)
            {
                var stringCommand = message.Split(_deliver).First().Split(" ").Last();
                Command command = Command.name;
                try
                {
                    Enum.TryParse(stringCommand, out command);
                }
                catch (Exception ex) {
                    _logger.WriteLog(ex.Message, LogType.Error);
                };

                var callMessageId = update.CallbackQuery.Message.MessageId;
                userContext.BotProjectUpdater = new BotProjectUpdate(userContext, _botClient, _projectService, update, Guid.Parse(message.Split(_deliver).Last()), command, _logger, callMessageId);
                userContext.BotProjectUpdater.UpdatedSuccessEvent += OnUpdated;
            }
            else if(message.Split(_deliver).Length == 2 && message.Split(_deliver).First() == "delete")
            {
                var projectId = Guid.Parse(message.Split(_deliver).Last());
                var projectModel = _projectService.ProjectGetAsync(projectId).Result;
                if (projectModel.OwnerId == userContext.User.UserId)
                {
                    _projectService.ProjectDelete(projectModel.Id);
                    if(update.CallbackQuery.Message is not null)
                    {
                        _botClient.EditMessageText(userContext.User.TelegramId, messageId: update.CallbackQuery.Message.MessageId, text: userContext.ResourceManager.GetString("ProjectHasBeenDeletedMes"));
                    }
                    else
                    {
                        _botClient.SendMessage(userContext.User.TelegramId, text: userContext.ResourceManager.GetString("ProjectHasBeenDeletedMes"));
                    }
                }
                else
                {
                    _botClient.SendMessage(userContext.User.TelegramId, userContext.ResourceManager.GetString("DenialOfAccess"));
                }
            }
        }

        private void ToCreateProject(Telegram.BotAPI.GettingUpdates.Update update, UserContextModel userContext)
        {
            if (userContext.BotProjectCreator is null)
            {
                //userContext.CreateProjectContext = new BotCreateProjectModel(userContext.User.UserId, userContext.User.TelegramId);

                userContext.BotProjectCreator = new BotProjectCreator(userContext, _botClient, _projectService, _logger);
                userContext.BotProjectCreator.CreatedSuccessEvent += OnCreated;
            }
            else _logger.WriteLog("User have no closed project context", LogType.Error);

            int messageId = update.Message is not null ? update.Message.MessageId : update.CallbackQuery.Message.MessageId;
            DeleteMessage(userContext.User.TelegramId, messageId);
        }

        private bool BotSleshCommandHendler(Telegram.BotAPI.GettingUpdates.Update update, UserContextModel userContext)
        {
            if (update.Message is null) return false;
            if (update.Message.Text is null) return false;
            if (update.Message.Text.First() != '/') return false;
            if (update.Message.Text.Split("/").Length != 2) return false;
            if (update.Message.Text.Split(" ").Length > 2) return false;
            //if (userContext.BotProjectCreator is not null || userContext.UpdateProjectContext is not null) return false;
            if (userContext.BotProjectCreator is not null) {
                userContext.BotProjectCreator.ClearMessages();
                userContext.BotProjectCreator = null;
                userContext.CreateProjectContext = null;
                _botClient.SendMessage(update.Message.Chat.Id, userContext.ResourceManager.GetString("CanceledOperation"));
            }
            else if (userContext.BotProjectUpdater is not null)
            {
                userContext.BotProjectUpdater.ClearMessages();
                userContext.BotProjectUpdater = null;
                userContext.UpdateProjectContext = null;
                _botClient.SendMessage(update.Message.Chat.Id, userContext.ResourceManager.GetString("CanceledOperation"));
            };
            if (update.Message.Text == "/menu")
            {
                DeleteMessage(userContext.User.TelegramId, update.Message.MessageId);
                _botClient.SendMessage(update.Message.Chat.Id, userContext.ResourceManager.GetString("MainMenu"), replyMarkup: Keyboards.MainMenu(userContext));
                return true;
            }
            else if (update.Message.Text == "/start")
            {
                var username = update.Message.Chat.FirstName ?? update.Message.Chat.Username ?? "";
                string welcomeMessage = string.Format(userContext.ResourceManager.GetString("Welcome"), username, _botClient.GetMe().Username);
                _botClient.SendMessage(update.Message.Chat.Id, welcomeMessage, replyMarkup: Keyboards.InlineStartMenuKeyboard(userContext), parseMode: "HTML");
                //_botClient.SendMessage(update.Message.Chat.Id, welcomeMessage, replyMarkup: Keyboards.MainMenu(userContext), parseMode: "HTML");
                return true;
            }
            else if (update.Message.Text == "/start addProject")
            {
                ToCreateProject(update, userContext);
                return true;
            }
            else if (update.Message.Text == "/about")
            {
                DeleteMessage(userContext.User.TelegramId, update.Message.MessageId);

                var ms = userContext.ResourceManager.GetString("About");
                _logger.WriteLog(ms, LogType.Information);
                SendMessageArgs inputMessage = new SendMessageArgs(update.Message.Chat.Id, ms);
                inputMessage.ParseMode = "HTML";

                _botClient.SendMessage(inputMessage);
                return true;

            }
            else if (update.Message.Text == "/test-exeption")
            {
                throw new Exception("test exeption");
            }
            else
            {
                var ms = userContext.ResourceManager.GetString("CommandNotRecognized");
                var formatedMs = String.Format(ms, update.Message.Text);

                SendMessageArgs inputMessage = new SendMessageArgs(update.Message.Chat.Id, formatedMs);
                inputMessage.ParseMode = "HTML";

                _botClient.SendMessage(inputMessage);
            }

            return false;
        }

        private void InlineSearch(Telegram.BotAPI.GettingUpdates.Update update, UserContextModel userContext)
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

            projects.AddRange(projects);

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

        private UserContextModel GetUserContext(Telegram.BotAPI.GettingUpdates.Update update)
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
            else if(update.InlineQuery is not null){
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
                if(update.Message is null && update.InlineQuery is null && update.CallbackQuery is null)
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

                string welcomeMessage = user.Username is not null || user.FirstName is not null ? String.Format(userContext.ResourceManager.GetString("WelcomeUserMessage"), userContext.User.Username) : userContext.ResourceManager.GetString("WelcomeMessage");

                try
                {
                    _botClient.SendMessage(chatId, welcomeMessage);
                }
                catch {
                    _logger.WriteLog("I can't send a greeting message", LogType.Warning);
                }

                _logger.WriteLog("Added new user: " + user.Username, LogType.Information);
            }

            if (!_usersContext.Exists(x => x.User.UserId == user.UserId)) _usersContext.Add(new UserContextModel(user));

            userContext = _usersContext.First(x => x.User.UserId == user.UserId);
            return userContext;
        }

        private void ViewUpdateMenu(Telegram.BotAPI.GettingUpdates.Update update, UserContextModel userContext, ProjectModel projectModel)
        {
            var replyMarkuppp = GetUpdateKeyboard(userContext, projectModel);

            var message = MessageWidgets.AboutProject(projectModel, userContext.User, userContext);
            message += "\n\n" + userContext.ResourceManager.GetString("UpdateProjectInitMessasge");

            if(update.CallbackQuery.Message is not null)
            {
                _botClient.EditMessageText(chatId: userContext.User.TelegramId, messageId: update.CallbackQuery.Message.MessageId, text: message, replyMarkup: replyMarkuppp, parseMode: "HTML");
            }
            else
            {
                _botClient.SendMessage(chatId: userContext.User.TelegramId, text: message, replyMarkup: replyMarkuppp, parseMode: "HTML");
            }
        }

        private InlineKeyboardMarkup GetUpdateKeyboard(UserContextModel userContext, ProjectModel projectModel)
        {
            var nameBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("Name"));
            var descriptionBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("Description"));
            var devStatusBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("Status"));
            var linksBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("Links"));
            nameBtn.CallbackData = _punktsMenu[0] + _deliver + projectModel.Id;
            descriptionBtn.CallbackData = _punktsMenu[1] + _deliver + projectModel.Id;
            devStatusBtn.CallbackData = _punktsMenu[2] + _deliver + projectModel.Id;
            linksBtn.CallbackData = _punktsMenu[3] + _deliver + projectModel.Id;

            var replyMarkuppp = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                    new InlineKeyboardButton[]{
                                                nameBtn,
                                                descriptionBtn,
                                                devStatusBtn,
                                                linksBtn,
                                            }
                }
            );

            return replyMarkuppp;
        }

        private InlineKeyboardMarkup GetProjectItemMenu(ProjectModel project, UserContextModel userContext)
        {
            string deliver = " mya~ ";

            var updateBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("UpdateBtn"));
            var deleteBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("DeleteBtn"));
            updateBtn.CallbackData = "update" + deliver + project.Id;
            deleteBtn.CallbackData = "delete" + deliver + project.Id;

            var replyMarkuppp = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                                            new InlineKeyboardButton[]{
                                                updateBtn,
                                                deleteBtn
                                            }
                }
            );
            return replyMarkuppp;
        }
    }
}
