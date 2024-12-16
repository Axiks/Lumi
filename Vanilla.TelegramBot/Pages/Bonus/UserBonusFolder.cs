using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.BotAPI;
using Vanilla.TelegramBot.Abstract;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages.Bonus.Pages;
using Vanilla_App.Services.Bonus;

namespace Vanilla.TelegramBot.Pages.Bonus
{
    internal class UserBonusFolder : AbstractFolder, IFolder
    {
        public UserBonusFolder(TelegramBotClient botClient, UserContextModel userContext, IUserService userService, ILogger logger, IBonusService bonusService) : base(botClient, userContext, userService, logger)
        {

            var userBonusPage = new UserBonusPage(botClient, userContext, _sendMessages, bonusService, _sendedMessages);

            var PagesCatalog = new List<IPage>
            {
                userBonusPage
            };

            this.InitPagesCatalog(PagesCatalog);
            this.InitPages(PagesCatalog);
        }
    }
}
