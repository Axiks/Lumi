using Telegram.BotAPI.AvailableTypes;
using Vanilla.TelegramBot.Models;
using Vanilla_App.Services.Projects;

namespace Vanilla.TelegramBot.UI
{
    public static class Keyboards
    {
        //private readonly static string[] _mainMenuitems = { "Add project", "View own projects" };
        public static ReplyKeyboardMarkup MainMenu(UserContextModel userContext)
        {
            KeyboardButton[][] mainMenuKeyboardButtons = new KeyboardButton[][]{
                                        new KeyboardButton[]{
                                            new KeyboardButton(userContext.ResourceManager.GetString("AddProject")),
                                         },// column 1
                                         new KeyboardButton[]{
                                             new KeyboardButton(userContext.ResourceManager.GetString("ViewOwnProjects")),
                                             new KeyboardButton(userContext.ResourceManager.GetString("MyProfile"))
                                          },
                                         new KeyboardButton[]{
                                            new KeyboardButton(userContext.ResourceManager.GetString("BonusSytemBtn")),
                                         },
      
                                    };
            var replyMarkup = new ReplyKeyboardMarkup(mainMenuKeyboardButtons);
            //replyMarkup.IsPersistent = true;

            return replyMarkup;
        }

        public static ReplyKeyboardMarkup ProfileKeyboard(UserContextModel userContext)
        {
            KeyboardButton[][] mainMenuKeyboardButtons = new KeyboardButton[][]{
                                         new KeyboardButton[]{
                                             new KeyboardButton(userContext.ResourceManager.GetString("MyProfileUpdate"))
                                             },
                                         new KeyboardButton[]{
                                                new KeyboardButton(userContext.ResourceManager.GetString("Back")),
                                            }
                                    };

            var replyMarkup = new ReplyKeyboardMarkup(mainMenuKeyboardButtons);
            replyMarkup.ResizeKeyboard = true;
            replyMarkup.IsPersistent = true;
            //replyMarkup.OneTimeKeyboard = true;

            return replyMarkup;
        }

        public static ReplyKeyboardMarkup CannelKeyboard(UserContextModel userContext, String placeholder = null)
        {
            var cannelKeyboardButtons = new KeyboardButton[][]{
                                            new KeyboardButton[]{
                                                new KeyboardButton(userContext.ResourceManager.GetString("Cannel")),
                                            }
                                        };
            var replyMarkup = new ReplyKeyboardMarkup(cannelKeyboardButtons);
            replyMarkup.ResizeKeyboard = true;
            //replyMarkup.OneTimeKeyboard = true;
            replyMarkup.IsPersistent = true;
            if (placeholder != null)
                replyMarkup.InputFieldPlaceholder = placeholder;
            return replyMarkup;
        }

        public static ReplyKeyboardMarkup BackKeyboard(UserContextModel userContext, String placeholder = null)
        {
            var cannelKeyboardButtons = new KeyboardButton[][]{
                                            new KeyboardButton[]{
                                                new KeyboardButton(userContext.ResourceManager.GetString("Back")),
                                            }
                                        };
            var replyMarkup = new ReplyKeyboardMarkup(cannelKeyboardButtons);
            replyMarkup.ResizeKeyboard = true;
            //replyMarkup.OneTimeKeyboard = true;
            replyMarkup.IsPersistent = true;
            if (placeholder != null)
                replyMarkup.InputFieldPlaceholder = placeholder;
            return replyMarkup;
        }

        public static ReplyKeyboardMarkup PassAndCannelKeyboard(UserContextModel userContext, String placeholder = null)
        {
            var cannelKeyboardButtons = new KeyboardButton[][]{
                new KeyboardButton[]{
                                                new KeyboardButton(userContext.ResourceManager.GetString("Pass")),
                                            },
                                            new KeyboardButton[]{
                                                new KeyboardButton(userContext.ResourceManager.GetString("Cannel")),
                                            }
                                        };
            var replyMarkup = new ReplyKeyboardMarkup(cannelKeyboardButtons);
            replyMarkup.ResizeKeyboard = true;
            if (placeholder != null)
                replyMarkup.InputFieldPlaceholder = placeholder;
            return replyMarkup;
        }

        public static InlineKeyboardMarkup InlineStartMenuKeyboard(UserContextModel userContext)
        {
            var AddProjectBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("AddProject"));
            AddProjectBtn.CallbackData = "AddProject";
            var MainMenuBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("MainMenu"));
            MainMenuBtn.CallbackData = "MainMenu";
            var SearchBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("Search"));
            SearchBtn.SwitchInlineQueryCurrentChat = "";

            var replyMarkuppp = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                    new InlineKeyboardButton[]{
                                                AddProjectBtn
                                            },
                    new InlineKeyboardButton[]{
                                                MainMenuBtn,
                                                SearchBtn
                                            },
                }
            );
            return replyMarkuppp;
        }

        public static InlineKeyboardMarkup GetPassKeypoard(UserContextModel userContext)
        {
            var passBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("Pass"));
            passBtn.CallbackData = "pass";

            var replyMarkuppp = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                    new InlineKeyboardButton[]{
                        passBtn
                    },
                }
            );

            return replyMarkuppp;
        }

        public static InlineKeyboardMarkup GetErrorKeypoard(UserContextModel userContext)
        {
            var topicLinkBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("Pass"));
            topicLinkBtn.Text = "Топік з багами";
            topicLinkBtn.Url = "https://t.me/LumiFanbase/2";

            var reloadUserContextBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("ReloadUserContext"));
            reloadUserContextBtn.CallbackData= "ReloadUserContext";

            var replyMarkuppp = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                    new InlineKeyboardButton[]{
                        topicLinkBtn
                    },
                    new InlineKeyboardButton[]{
                        reloadUserContextBtn
                    },
                }
            );

            return replyMarkuppp;
        }

        public static InlineKeyboardMarkup GetCreateProfileKeypoard(UserContextModel userContext)
        {
            var passBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("CreateProfile"));
            passBtn.CallbackData = "CreateProfile";

            var replyMarkuppp = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                    new InlineKeyboardButton[]{
                        passBtn
                    },
                }
            );

            return replyMarkuppp;
        }

        public static InlineKeyboardMarkup GetCreateProfileKeypoardWithSearch(UserContextModel userContext)
        {
            var passBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("CreateProfile"));
            passBtn.CallbackData = "CreateProfile";

            var SearchBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("Search"));
            SearchBtn.SwitchInlineQueryCurrentChat = "";

            var replyMarkuppp = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                    new InlineKeyboardButton[]{
                        passBtn
                    },
                    new InlineKeyboardButton[]{
                        SearchBtn
                    },
                }
            );

            return replyMarkuppp;
        }


        public static InlineKeyboardMarkup GetCreateProjectKeypoard(UserContextModel userContext)
        {
            var passBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("SaveProject"));
            passBtn.CallbackData = "SaveProject";

            var updateBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("UpdateBtn"));
            updateBtn.CallbackData = "update";

            var replyMarkuppp = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                    new InlineKeyboardButton[]{
                        passBtn
                    },
                    new InlineKeyboardButton[]{
                        updateBtn
                    },
                }
            );

            return replyMarkuppp;
        }

        public static InlineKeyboardMarkup GetInlineSearchProjectKeypoard(UserContextModel userContext, ProjectModel projectModel)
        {
            var SearchProjectBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("FindThisProjectBtn"));
            SearchProjectBtn.SwitchInlineQueryCurrentChat = projectModel.Name;


            var replyMarkuppp = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                    new InlineKeyboardButton[]{
                        SearchProjectBtn
                    },
                }
            );

            return replyMarkuppp;
        }

        public static InlineKeyboardMarkup InlineKeyboardConstructor(UserContextModel userContext, (string, string) button)
        {
            var btn = new InlineKeyboardButton(text: button.Item1);
            btn.CallbackData = button.Item2;

            var replyMarkup = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                    new InlineKeyboardButton[]{
                        btn
                    },
                }
            );

            return replyMarkup;
        }

        public static InlineKeyboardMarkup GetProjectInlineKeyboard(UserContextModel userContext, Guid projectId)
        {
            /*var SearchProjectBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("FindThisProjectBtn"));
            SearchProjectBtn.SwitchInlineQueryCurrentChat = projectModel.Name;*/

            string deliver = " mya~ ";

            var updateBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("UpdateBtn"));
            var deleteBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("DeleteBtn"));

            //updateBtn.CallbackData = "UpdateProjectFolder/UpdateProjectPage" + deliver + projectId;
            updateBtn.CallbackData = "update" + deliver + projectId.ToString();
            deleteBtn.CallbackData = "delete" + deliver + projectId.ToString();

            var replyMarkuppp = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                    new InlineKeyboardButton[]{
                        updateBtn,
                        deleteBtn
                    },
               /*     new InlineKeyboardButton[]{
                        SearchProjectBtn
                    },*/
                }
            );

            return replyMarkuppp;
        }

        public static InlineKeyboardMarkup GetCreateProjectInlineKeyboard(UserContextModel userContext)
        {
            var makeNewProjectBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("AddProject"));
            makeNewProjectBtn.CallbackData = "AddProject";

            var replyMarkuppp = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                    new InlineKeyboardButton[]{
                        makeNewProjectBtn
                    },
                }
            );

            return replyMarkuppp;
        }

        public static InlineKeyboardMarkup GetProjectUpdateItemsKeyboard(UserContextModel userContext)
        {
            List<string> _punktsMenu = new List<string>
                            {
                                "name",
                                "description",
                                "status",
                                "links",
                            };

            var nameBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("Name"));
            var descriptionBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("Description"));
            var devStatusBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("Status"));
            var linksBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("Links"));
            nameBtn.CallbackData = _punktsMenu[0];
            descriptionBtn.CallbackData = _punktsMenu[1];
            devStatusBtn.CallbackData = _punktsMenu[2];
            linksBtn.CallbackData = _punktsMenu[3];

            var replyMarkuppp = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                    new InlineKeyboardButton[]{
                        nameBtn,
                    },
                    new InlineKeyboardButton[]{
                        descriptionBtn,
                    },
                    new InlineKeyboardButton[]{
                        devStatusBtn,
                    },
                    new InlineKeyboardButton[]{
                        linksBtn,
                    }
                }
            );

            return replyMarkuppp;
        }

        public static InlineKeyboardMarkup GetProjectUpdateItemsKeyboard(UserContextModel userContext, Guid projectId)
        {
            List<string> _punktsMenu = new List<string>
                            {
                                "name",
                                "description",
                                "status",
                                "links",
                            };

            string _deliver = " mya~ ";


            var nameBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("Name"));
            var descriptionBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("Description"));
            var devStatusBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("Status"));
            var linksBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("Links"));
            nameBtn.CallbackData = _punktsMenu[0] + _deliver + projectId.ToString();
            descriptionBtn.CallbackData = _punktsMenu[1] + _deliver + projectId.ToString();
            devStatusBtn.CallbackData = _punktsMenu[2] + _deliver + projectId.ToString();
            linksBtn.CallbackData = _punktsMenu[3] + _deliver + projectId.ToString();

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

        public static InlineKeyboardMarkup CannelInlineKeyboard(UserContextModel userContext)
        {
            var cannelBtn = new InlineKeyboardButton(userContext.ResourceManager.GetString("Cannel"));
            cannelBtn.CallbackData = "cannel";

            var replyMarkuppp = new InlineKeyboardMarkup
           (
               new InlineKeyboardButton[][]{
                    new InlineKeyboardButton[]{
                                                cannelBtn
                                            }
               }
           );

            return replyMarkuppp;
        }


        /*        public static InlineKeyboardMarkup InlineKeyboardConstructor(UserContextModel userContext, List<(string, string)> buttons)
                {
                    List<List<InlineKeyboardButton>> rowWithKeysBonus = new List<List<InlineKeyboardButton>>();

                    foreach (var button in buttons)
                    {

                        var btn = new InlineKeyboardButton(text: button.Item1);
                        btn.CallbackData = button.Item2;

                        var btnRow = new List<InlineKeyboardButton> { btn };
                        rowWithKeysBonus.Add(btnRow);
                    }

                    return new InlineKeyboardMarkup
                    (
                        rowWithKeysBonus.ToArray()
                    );
                }*/

    }
}
