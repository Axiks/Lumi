using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.BotAPI.AvailableTypes;

namespace Vanilla.TelegramBot.UI
{
    public static class Keyboards
    {
        private readonly static string[] _mainMenuitems = { "Add project", "View own projects" };
        public static ReplyKeyboardMarkup MainMenu()
        {
            KeyboardButton[][] mainMenuKeyboardButtons = new KeyboardButton[][]{
                                        new KeyboardButton[]{
                                            new KeyboardButton(_mainMenuitems[0]),
                                            },// column 1
                                         new KeyboardButton[]{
                                             new KeyboardButton(_mainMenuitems[1])
                                             }
                                    };

            return new(mainMenuKeyboardButtons);
        }

        public static ReplyKeyboardMarkup CannelKeyboard()
        {
            var cannelKeyboardButtons = new KeyboardButton[][]{
                                            new KeyboardButton[]{
                                                new KeyboardButton("Cannel"),
                                            }
                                        };
             return new ReplyKeyboardMarkup(cannelKeyboardButtons);
        }
    }
}
