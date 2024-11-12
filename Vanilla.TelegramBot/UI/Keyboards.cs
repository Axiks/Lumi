using Telegram.BotAPI.AvailableTypes;
using Vanilla.TelegramBot.Models;

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
            var passBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("Pass"));
            passBtn.Text = "Топік з багами";
            passBtn.Url = "https://t.me/c/2378034660/2";

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
    }
}
