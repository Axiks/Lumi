using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.BotAPI;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.TelegramBot.Abstract;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages.UpdateUser.Pages;
using Vanilla_App.Models;

namespace Vanilla.TelegramBot.Pages.CreateUser
{
    internal class UserCreateProfileFolder : AbstractFolder, IFolder
    {
        const string? folderName = "Профіль";
        public UserCreateProfileFolder(TelegramBotClient botClient, UserContextModel userContext, IUserService userService, ILogger logger) : base(botClient, userContext, userService, logger, isWithCannelButton: false, folderName: folderName)
        {
            var userModel = userContext.UpdateUserContext;

            var PagesCatalog = new List<IPage>
            {
                new CreateUserWelcomePage(botClient, userContext, _sendedMessages),
                new UpdateUserNicknamePage(botClient, userContext, _sendMessages),
                new UpdateUserAboutPage(botClient, userContext, _sendMessages),
                new UpdateUserLinksPage(botClient, userContext, _sendMessages),
                new UpdateIsRedyToWorkPage(botClient, userContext, _sendMessages),
                new UpdateUserImagesPage(botClient, userContext, _sendMessages),
                new UpdateUserComplitePage(botClient, userContext, _sendMessages, userService),
                new UpdateSeccessUserPage(botClient, userContext, _sendMessages),
            };

            InitPagesCatalog(PagesCatalog);
            InitPages(PagesCatalog);

        }

        //public override List<IPage> _pagesCatalog { get => _pagesCatalog; }

    }
}
