using Microsoft.Extensions.Configuration;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.InlineMode;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Entityes;
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

        private readonly string[] mainMenuitems = { "Add project", "View own projects" };

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

            _botClient = new TelegramBotClient(_settings.BotAccessToken);

            _logger.WriteLog("Init bot service", LogType.Information);

        }

        public async Task StartListening()
        {
            //var x = _botClient.GetWebhookInfo;

            var me = _botClient.GetMe();
            Console.WriteLine("My name is {0}.", me.FirstName);
            Console.WriteLine($"Start listening for @{me.Username}");
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
                                if(messageText == mainMenuitems[0])
                                {
                                    ToCreateProject(update, currentUserContext);
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
                            var mess = String.Format("Oooh\r\n\r\nAn internal server error has occurred\r\nIf the bot does not work correctly in the future, please contact {0}\r\n\r\nError ID: <b>{1}</b>", "@Yumikki", exeptionId);
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

            if (!isUserHaveRunTask && update.Message.Text == mainMenuitems[0])
            {
                DeleteMessage(userContext.User.TelegramId, update.Message.MessageId);
                ToCreateProject(update, userContext);
                return true;
            }
            else if (!isUserHaveRunTask && update.Message.Text == mainMenuitems[1])
            {
                //DeleteMessage(userContext.User.TelegramId, update.Message.MessageId);
                ToViewUserProjets(update, userContext);
                return true;
            }
            else if (update.Message.Text == "Cannel")
            {
                DeleteMessage(userContext.User.TelegramId, update.Message.MessageId);

                if (userContext.BotProjectCreator is not null)
                {
                    // Reset create project
                    //_botClient.DeleteMessages(chatId: chatId, messageIds: userContext.CreateProjectContext.SendedMessages);
                    userContext.BotProjectCreator.ClearMessages();
                    userContext.CreateProjectContext = null;
                    userContext.BotProjectCreator = null;

                    _botClient.SendMessage(update.Message.Chat.Id, "Main menu", replyMarkup: Keyboards.MainMenu());
                }
                else if (userContext.BotProjectUpdater is not null)
                {
                    // Reset update project
                    userContext.BotProjectUpdater.ClearMessages();
                    userContext.BotProjectUpdater = null;

                    _botClient.SendMessage(update.Message.Chat.Id, "Main menu", replyMarkup: Keyboards.MainMenu());
                }
                else
                {
                    _botClient.SendMessage(update.Message.Chat.Id, "Main menu", replyMarkup: Keyboards.MainMenu());
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
            var message = MessageWidgets.AboutProject(project, userContext.User);
            var replyMarkuppp = GetProjectItemMenu(project);
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
            makeNewProjectBtn.CallbackData = mainMenuitems[0];

            var replyNoProjectMarkup = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                                            new InlineKeyboardButton[]{
                                                makeNewProjectBtn
                                            }
                }
            );

            if (userProjects == null || userProjects.Count() == 0) _botClient.SendMessage(chatId, "About such\r\nYou don't have any projects yet\r\n\r\nBut you can add it!", replyMarkup: replyNoProjectMarkup);


            foreach (var project in userProjects)
            {
                string deliver = " mya~ ";

                var replyMarkuppp = GetProjectItemMenu(project);

                _botClient.SendMessage(chatId, MessageWidgets.AboutProject(project, userContext.User),
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
                        _botClient.EditMessageText(userContext.User.TelegramId, messageId: update.CallbackQuery.Message.MessageId, text: "The project has been deleted");
                    }
                    else
                    {
                        _botClient.SendMessage(userContext.User.TelegramId, text: "The project has been deleted");
                    }
                }
                else
                {
                    _botClient.SendMessage(userContext.User.TelegramId, "Denial of access");
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

        /*private void BotMessageHendler(Telegram.BotAPI.GettingUpdates.Update botUpdate, UserContextModel userContext)
        {
            if (botUpdate.Message.Text is null) return;
            var text = botUpdate.Message.Text;
            long chatId = userContext.User.TelegramId;

            //var userProject = inCreateProjects.FirstOrDefault(x => x.UserId == user.UserId && (x.Name is null || x.Description is null || x.DevelopStatus is null || x.Links is null));
            //var userProject = userContext.CreateProjectContext;
          *//*  if (userProject is not null)
            {
                userProject.SendedMessages.Add(botUpdate.Message.MessageId);

                if (botUpdate.Message.ViaBot != null && botUpdate.Message.ViaBot.Id == _botClient.GetMe().Id)
                {
                    var mess = _botClient.SendMessage(botUpdate.Message.Chat.Id, "Munch\r\nThis is my message!!! \n<b>I won't miss it</b>\n\nWrite it yourself", parseMode: "HTML");
                    userProject.SendedMessages.Add(mess.MessageId);
                    return;
                }

                AddProjectProcess(userProject, text, chatId);
            }
            else
            {
                // Clear first message
                //_botClient.DeleteMessage(chatId, messageId: update.Message.MessageId);
            }*/

        /*var userUpdateProject = inUpdateProjects.FirstOrDefault(x => x.UserId == userContext.User.UserId);
        if (userUpdateProject is not null)
        {
            var selectedItem = userUpdateProject.SelectedItem;
            var updatedModel = new ProjectUpdateRequestModel
            {
                Id = userUpdateProject.ProjectId,
            };

            if (selectedItem == SelectedItem.Name) updatedModel.Name = text;
            else if (selectedItem == SelectedItem.Description) updatedModel.Description = text;
            *//* else if(selectedItem == SelectedItem.Status) updatedModel.Description = text;
             else if(selectedItem == SelectedItem.Description) updatedModel.Description = text;*//*


            _projectService.ProjectUpdateAsync(updatedModel);
        }*//*
    }*/

        private bool BotSleshCommandHendler(Telegram.BotAPI.GettingUpdates.Update update, UserContextModel userContext)
        {
            if (update.Message is null) return false;
            if (userContext.BotProjectCreator is not null || userContext.UpdateProjectContext is not null) return false;
            if (update.Message.Text == "/menu")
            {
                DeleteMessage(userContext.User.TelegramId, update.Message.MessageId);
                _botClient.SendMessage(update.Message.Chat.Id, "Main menu", replyMarkup: Keyboards.MainMenu());
                return true;
            }
            else if (update.Message.Text == "/start")
            {
                var username = update.Message.Chat.FirstName ?? update.Message.Chat.Username ?? "";
                string welcomeMessage = string.Format("Hi {0}! \nI'm glad to see you here\r\nHere you can create and manage your own projects\r\n\r\n🌟<b>To check people's projects, it is enough to type @{1} here, or in any other chat</b>🌟 \n\nI hope you like it here ❤️", username, _botClient.GetMe().Username);
                _botClient.SendMessage(update.Message.Chat.Id, welcomeMessage, replyMarkup: Keyboards.MainMenu(), parseMode: "HTML");
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

                SendMessageArgs inputMessage = new SendMessageArgs(update.Message.Chat.Id,
                    "Thank you for being with us ❤\nAlso thank people who helped to develop this bot\n\nYou can contact the author here: <a href=\"https://t.me/Yumikki\">@Yumikki</a>\nPS. If you are fascinated by the magic of programming, we will be happy to welcome you <a href=\"https://t.me/include_anime\">to our family</a> 😊");
                inputMessage.ParseMode = "HTML";

                _botClient.SendMessage(inputMessage);
                return true;

            }
            else if (update.Message.Text == "/test-exeption")
            {
                throw new Exception("test exeption");
            }

            return false;
        }

        private void InlineSearch(Telegram.BotAPI.GettingUpdates.Update update, UserContextModel userContext)
        {
            var inline = update.InlineQuery;
            var query = inline.Query;

            var inlineAddOwnProjectButton = new InlineQueryResultsButton
            {
                Text = "Add own project",
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

            int index = 0;
            foreach (var project in projects)
            {
                //var messageContent = AboutProjectFormating(project);
                var owner = _userService.GetUser(project.OwnerId).Result;
                var messageContent = MessageWidgets.AboutProject(project, owner);

                var inputMessage = new InputTextMessageContent(messageContent);
                inputMessage.ParseMode = "HTML";


                var desription = "@" + owner.Username + "\n" + project.Description;

                int messageMaxLenght = 4090;
                if (desription.Length >= messageMaxLenght)
                {
                    int howMuchMore = desription.Length - messageMaxLenght;
                    int abbreviation = desription.Length - howMuchMore - 3;
                    desription = "@" + owner.Username + "\n" + project.Description.Substring(0, abbreviation) + "...";
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

            _botClient.AnswerInlineQuery(inlineQueryId: inline.Id, results: res, button: inlineAddOwnProjectButton);
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


                /*  protected override void OnCallbackQuery(CallbackQuery cQuery)
                  {
                      var args = cQuery.Data.Split(' ');
                      if (cQuery.Message == null)
                      {
                          _botClient.AnswerCallbackQuery(cQuery.Id, "This button is no longer available", true, cacheTime: 99999);
                          return;
                      }
                      var demoInvoice = _db.DemoInvoices.GetConfiguration(cQuery.Message.Chat.Id, cQuery.Message.MessageId);
                      if (demoInvoice == null)
                      {
                          _botClient.AnswerCallbackQuery(cQuery.Id, "This button is no longer available", true, cacheTime: 99999);
                          return;
                      }

                      void UpdateDemoInvoice()
                      {
                          _botClient.AnswerCallbackQuery(cQuery.Id, cacheTime: 5);

                          var keyboard = demoInvoice.GenInlineKeyboard();
                          _botClient.EditMessageReplyMarkup(new EditMessageReplyMarkup
                          {
                              ChatId = cQuery.Message.Chat.Id,
                              MessageId = cQuery.Message.MessageId,
                              ReplyMarkup = keyboard
                          });

                          _db.DemoInvoices.Update(demoInvoice);
                      }

                      switch (args[0])
                      {
                          case "switch":
                              {
                                  switch (args[1])
                                  {
                                      case "needName":
                                          demoInvoice.NeedName = !demoInvoice.NeedName;
                                          break;
                                      case "needEmail":
                                          demoInvoice.NeedEmail = !demoInvoice.NeedEmail;
                                          break;
                                      case "needPhone":
                                          demoInvoice.NeedPhone = !demoInvoice.NeedPhone;
                                          break;
                                      case "needShipping":
                                          demoInvoice.NeedShippingAddress = !demoInvoice.NeedShippingAddress;
                                          break;
                                      case "sendPhoto":
                                          demoInvoice.SendPhoto = !demoInvoice.SendPhoto;
                                          break;
                                      case "sendWebview":
                                          demoInvoice.SendWebView = !demoInvoice.SendWebView;
                                          break;
                                      default:
                                          _botClient.AnswerCallbackQuery(cQuery.Id, "???", cacheTime: 999);
                                          return;
                                  }
                                  UpdateDemoInvoice();
                              }
                              break;
                          case "setCurrency":
                              demoInvoice.Currency = Enum.Parse<CurrencyCodes>(args[1], true);
                              UpdateDemoInvoice();
                              break;
                          case "sendInvoice":
                              {
                                  _botClient.AnswerCallbackQuery(cQuery.Id, cacheTime: 2);
                                  _botClient.EditMessageText(cQuery.Message.Chat.Id, cQuery.Message.MessageId, MSG.DemoInvoiceResult, ParseMode.HTML, replyMarkup: null);

                                  const string productName = "Working Time Machine";
                                  const string description = "Want to visit your great-great-great-grandparents? Make a fortune at the races? Shake hands with Hammurabi and take a stroll in the Hanging Gardens? Order our Working Time Machine today!";

                                  var prices = new LabeledPrice[]
                                  {
                                      new LabeledPrice("Subtotal", 12345),
                                      new LabeledPrice("Handling", 5431),
                                      new LabeledPrice("Discount", -3454)
                                  };

                                  var invoice = new SendInvoiceArgs(cQuery.Message.Chat.Id, productName, description, "WorkingTimeMachine", Properties.ProviderToken, demoInvoice.Currency.ToString(), prices)
                                  {
                                      // ProviderData = demoInvoice.SendWebView ? "<Your provider data to use WebView>" : null,
                                      NeedName = demoInvoice.NeedName,
                                      NeedEmail = demoInvoice.NeedEmail,
                                      NeedPhoneNumber = demoInvoice.NeedPhone,
                                      NeedShippingAddress = demoInvoice.NeedShippingAddress,
                                      IsFlexible = demoInvoice.NeedShippingAddress,
                                      StartParameter = "buy_tshirt"
                                  };
                                  if (demoInvoice.SendPhoto)
                                  {
                                      const string photoUrl = "https://telegra.ph/file/a242b4418901347c47be6.jpg";
                                      invoice.PhotoUrl = photoUrl;
                                      invoice.PhotoSize = 51488;
                                      invoice.PhotoHeight = 490;
                                      invoice.PhotoWidth = 640;
                                  }

                                  try
                                  {
                                      _botClient.SendInvoice(invoice);
                                  }
                                  catch (BotRequestException exp)
                                  {
                                      var error = string.Format(MSG.SendInvoiceError, exp.Message);
                                      _botClient.SendMessage(cQuery.Message.Chat.Id, error, ParseMode.HTML);
                                  }

                                  _db.DemoInvoices.Delete(demoInvoice);
                              }
                              break;
                          default:
                              _botClient.AnswerCallbackQuery(cQuery.Id, "???", cacheTime: 999);
                              break;
                      }
                  }*/


            }
        }

        private UserContextModel GetUserContext(Telegram.BotAPI.GettingUpdates.Update update)
        {
            Models.UserModel user;

            long chatId = -1;

            string? username = null;
            string? firstname = null;
            string? lastname = null;

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
            }
            else if (update.PollAnswer is not null)
            {
                chatId = update.PollAnswer.User.Id;

                username = update.PollAnswer.User.Username;
                firstname = update.PollAnswer.User.FirstName;
                lastname = update.PollAnswer.User.LastName;
            }
            else if (update.CallbackQuery is not null)
            {
                chatId = update.CallbackQuery.From.Id;

                username = update.CallbackQuery.From.Username;
                firstname = update.CallbackQuery.From.FirstName;
                lastname = update.CallbackQuery.From.LastName;
            }
            else if (update.ChosenInlineResult is not null)
            {
                chatId = update.ChosenInlineResult.From.Id;

                username = update.ChosenInlineResult.From.Username;
                firstname = update.ChosenInlineResult.From.FirstName;
                lastname = update.ChosenInlineResult.From.LastName;
            }
            else
            {
                _logger.WriteLog("Don`t find user tg id", LogType.Error);
                throw new Exception("Don`t find user tg id");
            }

            try
            {
                user = _userService.SignInUser(chatId).Result;
            }
            catch
            {
                if(update.Message is null)
                {
                    _logger.WriteLog("Unable to create user", LogType.Error);
                    throw new Exception("Unable to create user");
                } 

                user = _userService.RegisterUser(new UserRegisterModel
                {
                    TelegramId = chatId,
                    Username = username,
                    FirstName = firstname,
                    LastName = lastname
                }).Result;

                string welcomeMessage = user.Username is not null || user.FirstName is not null ? "Welcome: " + user.Username ?? user.FirstName + "!" : "Welcome!";

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
            var currentUserContext = _usersContext.First(x => x.User.UserId == user.UserId);
            return currentUserContext;
        }

        private void ViewUpdateMenu(Telegram.BotAPI.GettingUpdates.Update update, UserContextModel userContext, ProjectModel projectModel)
        {
            var replyMarkuppp = GetUpdateKeyboard(projectModel);

            var message = MessageWidgets.AboutProject(projectModel, userContext.User);
            message += "\n" + "What do you want to update?";

            if(update.CallbackQuery.Message is not null)
            {
                _botClient.EditMessageText(chatId: userContext.User.TelegramId, messageId: update.CallbackQuery.Message.MessageId, text: message, replyMarkup: replyMarkuppp, parseMode: "HTML");
            }
            else
            {
                _botClient.SendMessage(chatId: userContext.User.TelegramId, text: message, replyMarkup: replyMarkuppp, parseMode: "HTML");
            }
        }

        private InlineKeyboardMarkup GetUpdateKeyboard(ProjectModel projectModel)
        {
            var nameBtn = new InlineKeyboardButton(text: "Name");
            var descriptionBtn = new InlineKeyboardButton(text: "Description");
            var devStatusBtn = new InlineKeyboardButton(text: "Status");
            var linksBtn = new InlineKeyboardButton(text: "Links");
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

        private InlineKeyboardMarkup GetProjectItemMenu(ProjectModel project)
        {
            string deliver = " mya~ ";

            var updateBtn = new InlineKeyboardButton(text: "Update ");
            var deleteBtn = new InlineKeyboardButton(text: "Delete ");
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
