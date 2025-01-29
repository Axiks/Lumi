using Telegram.BotAPI;
using Vanilla.TelegramBot.Abstract;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages.UpdateUser.Models;
using Vanilla.TelegramBot.Pages.UpdateUser.Pages;

namespace Vanilla.TelegramBot.Pages.CreateUser
{
    internal class UserCreateProfileFolder : AbstractFolder, IFolder
    {
        const string? folderName = "Створення профілю";
        public UserCreateProfileFolder(TelegramBotClient botClient, UserContextModel userContext, IUserService userService, Vanilla.TelegramBot.Interfaces.ILogger logger) : base(botClient, userContext, userService, logger, isWithCannelButton: false, folderName: folderName)
        {
            //var userModel = userContext.UpdateUserContext;
            var UserActionContextModel = new UserActionContextModel() { 
                Username = userContext.UpdateUser.Username
            };

            var PagesCatalog = new List<IPage>
            {
                //new CreateUserWelcomePage(botClient, userContext, _sendedMessages),
                new UpdateUserNicknamePage(botClient, userContext, _sendMessages, UserActionContextModel),
                new UpdateUserAboutPage(botClient, userContext, _sendMessages, UserActionContextModel),
                new UpdateUserLinksPage(botClient, userContext, _sendMessages, UserActionContextModel),
                new UpdateIsRedyToWorkPage(botClient, userContext, _sendMessages, UserActionContextModel),
                new UpdateUserImagesPage(botClient, userContext, _sendMessages, UserActionContextModel),
                //new UpdateUserComplitePage(botClient, userContext, _sendMessages, userService, updateUserActionContextModel),
                new CreateUserComplitePage(botClient, userContext, _sendMessages, userService, UserActionContextModel),
                new UpdateSeccessUserPage(botClient, userContext, userService, _sendMessages),
            };

            InitPagesCatalog(PagesCatalog);
            InitPages(PagesCatalog);

        }

        //public override List<IPage> _pagesCatalog { get => _pagesCatalog; }

    }
}
