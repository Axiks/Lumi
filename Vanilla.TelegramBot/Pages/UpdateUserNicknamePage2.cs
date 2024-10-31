using System;
using System.Linq;
using Telegram.BotAPI;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.TelegramBot.Abstract;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Pages
{
    public class UpdateUserNicknamePage2 : PageAbstractClass<BotUpdateUserModel>
    {
        public UpdateUserNicknamePage2(TelegramBotClient botClient, UserContextModel userContext, List<int> sendMessages, BotUpdateUserModel dataContext) : base(botClient, userContext, sendMessages, dataContext)
        {
        }

        internal override void Action(Update update)
        {
            throw new NotImplementedException();
        }

        internal override void SendInitMessage()
        {
            throw new NotImplementedException();
        }

        internal override void ValidateInputData(Update update)
        {
            throw new NotImplementedException();
        }

        internal override void ValidateInputType(Update update)
        {
            throw new NotImplementedException();
        }
    }
}
