using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.BotAPI;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages.UpdateUser;
using Vanilla.TelegramBot.Pages.User;

namespace Vanilla.TelegramBot.Pages
{
    internal class UserGetProfileFolder : AbstractFolder, IFolder
    {
        public UserGetProfileFolder(TelegramBotClient botClient, UserContextModel userContext, IUserService userService, ILogger logger) : base(botClient, userContext, userService, logger)
        {
            UserModel userModel = userContext.User;

            var infoPage = new UserInfoPage(botClient, userContext, userModel, this._sendMessages);

            var PagesCatalog = new List<IPage>
            {
                infoPage
            };

            this.InitPagesCatalog(PagesCatalog);
            this.InitPages(PagesCatalog);
        }
    }
}
