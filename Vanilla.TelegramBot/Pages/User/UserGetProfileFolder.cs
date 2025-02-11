﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.BotAPI;
using Vanilla.TelegramBot.Abstract;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages.UpdateUser;

namespace Vanilla.TelegramBot.Pages.User
{
    internal class UserGetProfileFolder : AbstractFolder, IFolder
    {
        const string? folderName = "Профіль";
        public UserGetProfileFolder(TelegramBotClient botClient, UserContextModel userContext, IUserService userService, Vanilla.TelegramBot.Interfaces.ILogger logger) : base(botClient, userContext, userService, logger, folderName: folderName)
        {
            UserModel userModel = userContext.User;

            var infoPage = new UserInfoPage(botClient, userContext, userModel, _sendedMessages);

            var PagesCatalog = new List<IPage>
            {
                infoPage
            };

            InitPagesCatalog(PagesCatalog);
            InitPages(PagesCatalog);
        }
    }
}
