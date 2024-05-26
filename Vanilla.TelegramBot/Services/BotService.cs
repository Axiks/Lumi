﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.InlineMode;
using Telegram.BotAPI.Extensions;
using Telegram.BotAPI.AvailableTypes;
using static System.Net.Mime.MediaTypeNames;
using Vanilla_App.Interfaces;
using Vanilla_App.Services;
using System.Text.RegularExpressions;
using Vanilla.Common.Enums;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla_App.Models;

namespace Vanilla.TelegramBot.Services
{
    public class BotService : IBotService
    {
        private readonly TelegramBotClient _botClient;
        private readonly SettingsModel _settings;
        //private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;

        private readonly ILogger _logger;

        private readonly string[] mainMenuitems = { "Add project", "View own projects", "Discovery projects" };

        private List<BotCreateProjectModel> inCreateProjects = new List<BotCreateProjectModel>();
        private List<BotUpdateProjectModel> inUpdateProjects = new List<BotUpdateProjectModel>();

        public BotService(IUserService userService, IProjectService projectService, ILogger logger) {
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
            var me = _botClient.GetMe();
            Console.WriteLine("My name is {0}.", me.FirstName);

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.WriteLine($"https://t.me/{me.Username}");

            var x = new List<InlineQueryResult> {
                new InlineQueryResultArticle
                {
                    Title = "SUper project 1",
                }
            };
            //_botClient.AnswerInlineQuery(new AnswerInlineQueryArgs("", x));

            var updates = _botClient.GetUpdates();
            while (true)
            {
                if (updates.Any())
                {

                    foreach (var update in updates)
                    {
                        if (update.Message is not null)
                        {
                            var mes = update.Message;
                            var text = mes.Text;

                            long chatId = update.Message.Chat.Id; // Target chat Id

                            // Registration new member
                            Models.UserModel user;
                            try
                            {
                                user = _userService.SignInUser(chatId).Result;
                            }
                            catch
                            {
                                user = _userService.RegisterUser(new UserRegisterModel
                                {
                                    TelegramId = chatId,
                                    Username = update.Message.Chat.Username,
                                    FirstName = update.Message.Chat.FirstName,
                                    LastName = update.Message.Chat.LastName
                                }).Result;

                                _botClient.SendMessage(chatId, "Welcome in our familly: " + user.Username); // Send a message
                            }

                           

                            if (update.Message.Text.Contains("/menu"))
                            {
                                var keyboardButtons = new KeyboardButton[][]{
                                            new KeyboardButton[]{
                                                new KeyboardButton(mainMenuitems[0]), //column 1 row 1
                                                new KeyboardButton(mainMenuitems[1]) //column 1 row 2
                                                },// column 1
                                            new KeyboardButton[]{
                                                new KeyboardButton(mainMenuitems[2]) //col 2 row 1
                                                } // column 2
                                        };

                                var keyboard = new ReplyKeyboardMarkup(keyboardButtons);
                                _botClient.SendMessage(update.Message.Chat.Id, "Main menu", replyMarkup: keyboard);
                            }

                            if (update.Message.Text.Contains("/about"))
                            {
                                SendMessageArgs inputMessage = new SendMessageArgs(update.Message.Chat.Id,
                                    "Thank you for being with us ❤\nAlso thank people who helped to develop this bot\n\nYou can contact the author here: <a href=\"https://t.me/Yumikki\">@Yumikki</a>\nPS. If you are fascinated by the magic of programming, we will be happy to welcome you <a href=\"https://t.me/include_anime\">to our family</a> 😊");
                                inputMessage.ParseMode = "HTML";

                                _botClient.SendMessage(inputMessage);

                            }




                            var userProject = inCreateProjects.FirstOrDefault(x => x.UserId == user.UserId && (x.Name is null || x.Description is null || x.DevelopStatus is null || x.Links is null));
                            if (userProject is not null)
                            {
                                AddProjectProcess(userProject, text, chatId);
                                userProject.MessagesWhenCreating.Add(update.Message.MessageId);
                            }
                            else
                            {
                                // Clear first message
                                //_botClient.DeleteMessage(chatId, messageId: update.Message.MessageId);
                            }

                            var userUpdateProject = inUpdateProjects.FirstOrDefault(x => x.UserId == user.UserId);
                            if (userUpdateProject is not null)
                            {
                                var selectedItem = userUpdateProject.SelectedItem;
                                var updatedModel = new ProjectUpdateRequestModel
                                {
                                    Id = userUpdateProject.ProjectId,
                                };

                                if (selectedItem == SelectedItem.Name) updatedModel.Name = text;
                                else if(selectedItem == SelectedItem.Description) updatedModel.Description = text;
                               /* else if(selectedItem == SelectedItem.Status) updatedModel.Description = text;
                                else if(selectedItem == SelectedItem.Description) updatedModel.Description = text;*/


                                _projectService.ProjectUpdateAsync(updatedModel);
                            }

                            if (update.Message.Text.Contains(mainMenuitems[0]))
                            {
                                if (userProject is null)
                                {
                                    var userNewProject = new BotCreateProjectModel(user.UserId, user.TelegramId);

                                    inCreateProjects.Add(userNewProject);

                                    var keyboardButtons = new KeyboardButton[][]{
                                            new KeyboardButton[]{
                                                new KeyboardButton("Cannel"), //column 1 row 1
                                            }
                                        };

                                    var keyboard = new ReplyKeyboardMarkup(keyboardButtons);

                                    var messInit = _botClient.SendMessage(update.Message.Chat.Id, "What is the name of your wonderful project?", replyMarkup: keyboard);
                                    userNewProject.MessagesWhenCreating.Add(messInit.MessageId);
                                }
                            }

                            if (update.Message.Text.Contains("View own projects"))
                            {
                                // temp fix
                                var userProjects = _projectService.ProjectGetAllAsync().Result.Where(x => x.OwnerId == user.UserId);


                                if (userProjects == null || userProjects.Count() == 0) _botClient.SendMessage(chatId, "About such\r\nYou don't have any projects yet\r\n\r\nBut you can add it!");
                            

                                foreach (var project in userProjects)
                                {
                                    string links = "";
                                    foreach (var link in project.Links)
                                    {
                                        links += link + " \n";
                                    }

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

                                    _botClient.SendMessage(chatId, String.Format("Name: {0} \nDescription: {1} \nLinks: {2}",
                                        project.Name,
                                        project.Description,
                                        links),
                                        replyMarkup: replyMarkuppp);
                                }

                            }

                            if (update.Message.Text.Contains("Cannel"))
                            {
                                if (userProject is not null)
                                {

                                    // Reset create project
                                    _botClient.DeleteMessages(chatId: chatId, messageIds: userProject.MessagesWhenCreating);
                                    inCreateProjects.Remove(userProject);

                                    var keyboardButtons = new KeyboardButton[][]{
                                        new KeyboardButton[]{
                                            new KeyboardButton(mainMenuitems[0]), //column 1 row 1
                                            new KeyboardButton(mainMenuitems[1]) //column 1 row 2
                                            },// column 1
                                        new KeyboardButton[]{
                                            new KeyboardButton(mainMenuitems[2]) //col 2 row 1
                                            } // column 2
                                    };

                                    var keyboard = new ReplyKeyboardMarkup(keyboardButtons);
                                    _botClient.SendMessage(update.Message.Chat.Id, "Main menu", replyMarkup: keyboard);
                                }
                            }

                            string pattern = @"^project\s{1}[{(]?[0-9a-fA-F]{8}[-]?[0-9a-fA-F]{4}[-]?[0-9a-fA-F]{4}[-]?[0-9a-fA-F]{4}[-]?[0-9a-fA-F]{12}[)}]?$";
                            Regex regex = new Regex(pattern);

                            if (regex.IsMatch(update.Message.Text))
                            {
                                Guid projectId = Guid.Parse(update.Message.Text.Split(' ')[1]);

                                // temp fix
                                var project = _projectService.ProjectGetAsync(projectId).Result;

                                string links = "";
                                foreach (var link in project.Links)
                                {
                                    links += link + " \n";
                                }

                                _botClient.SendMessage(chatId, String.Format("Name: {0} \nDescription: {1} \nLinks: {2}",
                                    project.Name,
                                    project.Description,
                                    links));

                            }

                            /*if (update.Message.Text.Contains("project"))
                            {
                                var projectId = update.Message.Text.Split(' ')[1];

                                var id = Guid.Parse(projectId);

                                var project = _projectService.ProjectGetAsync(id).Result;

                                string links = "";
                                foreach (var link in project.Links)
                                {
                                    links += link + " \n";
                                }

                                _botClient.SendMessage(chatId, String.Format("Name: {0} \nDescription: {1} \nLinks: {2}",
                                        project.Name,
                                        project.Description,
                                        links));
                            }*/

                        }
                        else if (update.PollAnswer is not null)
                        {
                            var poll = update.PollAnswer;

                            var user = _userService.SignInUser(poll.User.Id).Result;
                            var userProject = inCreateProjects.FirstOrDefault(x => x.PollIdDevelopmentStatus == poll.PollId && x.UserId == user.UserId && x.DevelopStatus is null);
                            if (userProject is not null)
                            {
                                var optionIndex = poll.OptionIds.First();
                                var statusAsList = Enum.GetValues(typeof(DevelopmentStatusEnum)).Cast<DevelopmentStatusEnum>().ToList();

                                var selectedOption = statusAsList[optionIndex];
                                userProject.DevelopStatus = selectedOption;

                                //
                                var messPoll = _botClient.SendMessage(poll.User.Id, "List the link to this project via comma");
                                userProject.MessagesWhenCreating.Add(messPoll.MessageId);
                            }
                        }
                        else if (update.InlineQuery is not null)
                        {
                            var inline = update.InlineQuery;
                            var query = inline.Query;

                            var inlineAddOwnProjectButton = new InlineQueryResultsButton
                            {
                                Text = "Add own project",
                                StartParameter = "addProject"
                            };

                            var res = new List<InlineQueryResult>();

                            var projects = _projectService.ProjectGetAllAsync().Result;

                            int index = 0;
                            foreach(var project in projects)
                            {
                                var author = _userService.GetUser(project.OwnerId);
                                string links = "";
                                foreach (var link in project.Links)
                                {
                                    Uri linkUri = new Uri(link);
                                    //![👍](tg://emoji?id=5368324170671202286)
                                    links += "<a href=\"" + linkUri + "\">" + linkUri.Host.ToString() + "</a>" + "\n";
                                }

                                var messageContent = String.Format("<b>{0}</b> \n{1} \n\n{2}\n{3}\n{4}",
                                        project.Name,
                                        project.Description,
                                        "<i>"+project.DevelopmentStatus.ToString()+"</i>",
                                        links,
                                        "@" + author.Result.Username
                                        );
                                var inputMessage = new InputTextMessageContent(messageContent);
                                inputMessage.ParseMode = "HTML";

                                res.Add(new InlineQueryResultArticle
                                {
                                    Id = index.ToString(),
                                    Title = project.Name,
                                    Description = project.Description,
                                    //InputMessageContent = new InputTextMessageContent("project " + project.Id.ToString()),
                                    InputMessageContent = inputMessage,
                                });
                                index++;
                            }

                            var ans = new AnswerInlineQueryArgs(inline.Id, res);

                            _botClient.AnswerInlineQuery(inlineQueryId: inline.Id, results: res, button: inlineAddOwnProjectButton);
                        }
                        else if (update.CallbackQuery is not null)
                        {
                            string deliver = " mya~ ";
                            var punktsMenu = new List<string>
                            {
                                "update name",
                                "update description",
                                "update status",
                                "update links",
                            };

                            var callbackData = update.CallbackQuery.Data.Split(deliver);
                            var callbackCommand = callbackData.FirstOrDefault();

                            var callbackId = callbackData.LastOrDefault();
                            var projectId = Guid.Parse(callbackId);

                            var project = _projectService.ProjectGetAsync(projectId).Result;
                            var userId = update.CallbackQuery.From.Id;
                            var user = _userService.SignInUser(userId).Result;

                            if (callbackCommand.Contains("update") && callbackCommand.Split(" ").Count() == 1)
                            {
                                var nameBtn = new InlineKeyboardButton(text: "Name");
                                var descriptionBtn = new InlineKeyboardButton(text: "Description");
                                var devStatusBtn = new InlineKeyboardButton(text: "Status");
                                var linksBtn = new InlineKeyboardButton(text: "Links");
                                nameBtn.CallbackData = punktsMenu[0] + deliver + project.Id;
                                descriptionBtn.CallbackData = punktsMenu[1] + deliver + project.Id;
                                devStatusBtn.CallbackData = punktsMenu[2] + deliver + project.Id;
                                linksBtn.CallbackData = punktsMenu[3] + deliver + project.Id;

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


                                string links = "";
                                foreach (var link in project.Links)
                                {
                                    links += link + " \n";
                                }

                                var message = String.Format("Name: {0} \nDescription: {1} \nLinks: {2}",
                                        project.Name,
                                        project.Description,
                                        links);

                                message += "\n" + "What do you want to update?";

                                _botClient.EditMessageText(chatId: userId, messageId: update.CallbackQuery.Message.MessageId, text: message, replyMarkup: replyMarkuppp);
                            }
                            else if (callbackCommand.Contains("update") && callbackCommand.Split(" ").Count() == 2)
                            {
    
                                // Write routes
                                var userProject = inUpdateProjects.FirstOrDefault(x => x.UserId == user.UserId);
                                if(userProject is not null) inUpdateProjects.Remove(userProject);

                                SelectedItem selectedItem;

                                if (update.CallbackQuery.Data.Contains(punktsMenu[0] + deliver)) selectedItem = SelectedItem.Name;
                                else if (update.CallbackQuery.Data.Contains(punktsMenu[1] + deliver)) selectedItem = SelectedItem.Description;
                                else if (update.CallbackQuery.Data.Contains(punktsMenu[2] + deliver)) selectedItem = SelectedItem.Status;
                                else selectedItem = SelectedItem.Links;

                                inUpdateProjects.Add(new BotUpdateProjectModel(user.UserId, projectId, selectedItem, update.CallbackQuery.InlineMessageId));
                                _botClient.SendMessage(userId, "What value to replace?");

                            }
                            else if (callbackCommand is "delete")
                            {

                                if (project.OwnerId == user.UserId)
                                {
                                    _projectService.ProjectDelete(projectId);
                                    _botClient.EditMessageText(chatId: userId, messageId: update.CallbackQuery.Message.MessageId, text: "The project has been deleted");
                                }
                                else
                                {
                                    _botClient.SendMessage(userId, "Denial of access");
                                }

                            }
                        }
                        // Process update
                        //update.
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

        private void AddProjectProcess(BotCreateProjectModel userProject, string message, long chatId)
        {
            // Save name
            if (userProject.Name is null)
            {
                userProject.Name = message;

                var messName = _botClient.SendMessage(chatId, "Tell us more about it");
                userProject.MessagesWhenCreating.Add(messName.MessageId);
                return;
            }

            if (userProject.Description is null)
            {
                userProject.Description = message;

                var pullOptions = new List<InputPollOption>
                {
                    new InputPollOption
                    {
                        Text = DevelopmentStatusEnum.InDevelopment.ToString(),
                    },
                    new InputPollOption
                    {
                        Text = DevelopmentStatusEnum.Developed.ToString()
                    },
                    new InputPollOption
                    {
                        Text = DevelopmentStatusEnum.PlannedToDevelop.ToString()
                    },
                    new InputPollOption
                    {
                        Text = DevelopmentStatusEnum.Abandoned.ToString()
                    }
                };

                var pollParameters = new SendPollArgs(chatId, "Select the completion project status", pullOptions);
                pollParameters.IsAnonymous = false;
                var sendedMessage = _botClient.SendPoll(pollParameters);
                userProject.PollIdDevelopmentStatus = sendedMessage.Poll.Id;

                userProject.MessagesWhenCreating.Add(sendedMessage.MessageId);

                return;
            }
            if (userProject.DevelopStatus is null)
            {  

                return;
            }
            if (userProject.Links is null)
            {
                userProject.Links = new List<string>();

                var links = message.Split(",");
                userProject.Links.AddRange(links);

                _botClient.SendMessage(chatId, "I successfully added your project :3");

                //echo information about project
                var messLinks = _botClient.SendMessage(chatId, String.Format("Name: {0} \nDescription: {1} \nEthap: {2} \nLinks: {3}",
                    userProject.Name,
                    userProject.Description,
                    userProject.DevelopStatus,
                    userProject.Links));

                //userProject.MessagesWhenCreating.Add(messLinks.MessageId);

                var user = _userService.SignInUser(chatId).Result;

                var project = _projectService.ProjectCreateAsync(user.UserId, new Vanilla_App.Models.ProjectCreateRequestModel
                {
                    Name = userProject.Name,
                    Description = userProject.Description,
                    DevelopStatus = (DevelopmentStatusEnum) userProject.DevelopStatus,
                    Links = userProject.Links
                }).Result;

                // Clear messages
                _botClient.DeleteMessages(chatId: chatId, messageIds: userProject.MessagesWhenCreating);

                // Clear "cashe"
                inCreateProjects.Remove(userProject);

                return;
            }


        }

        private void UpdateProjectProcess()
        {

        }


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
