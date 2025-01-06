using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.BotAPI.GettingUpdates;

namespace Vanilla.TelegramBot.Helpers
{
    public static class ValidatorHelpers
    {
        public static bool TextValidate(Update update)
        {
            if (update.Message is not null && update.Message.Text is not null) return true;
            return false;
        }

        public static bool InlineBtnActionValidate(Update update, string command) {
            if (update.Message is not null && update.Message.Text is not null)
            {
                return update.Message.Text == command;
            }
            else return false;
        }

        public static bool CallbackBtnActionValidate(Update update, string command)
        {
            if (update.CallbackQuery is not null && update.CallbackQuery.Data is not null)
            {
                return update.CallbackQuery.Data == command;
            }
            else return false;
        }

        public static bool PoolAnswerValidate(Update update)
        {
            if (update.PollAnswer is not null) return true;
            return false;
        }
    }
}
