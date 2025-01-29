using Telegram.BotAPI;
using Vanilla.TelegramBot.Abstract;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages.UpdateUser.Pages;

namespace Vanilla.TelegramBot.Pages.UpdateUser
{
    public class UserUpdateProfileFolder : AbstractFolder, IFolder
    {
        const string? folderName = "Оновлення профілю";
        public UserUpdateProfileFolder(TelegramBotClient botClient, UserContextModel userContext, IUserService userService, Vanilla.TelegramBot.Interfaces.ILogger logger) : base(botClient, userContext, userService, logger, folderName: folderName)
        {
            //var userModel = userContext.UpdateUserContext;
            var UserActionContextModel = new UserActionContextModel()
            {
                Username = userContext.UpdateUser.Username
            };

            if (userContext.User != null) {
                UserActionContextModel = new UserActionContextModel
                {
                    Username = userContext.UpdateUser.Username,
                    Nickname = userContext.User.Nickname,
                    About = userContext.User.About,
                    Links = userContext.User.Links,
                    IsRadyForOrders = userContext.User.IsRadyForOrders,
                    Images = userContext.User.Images,
                };
            }

            var PagesCatalog = new List<IPage>
            {
                new UpdateUserNicknamePage(botClient, userContext, _sendMessages, UserActionContextModel),
                new UpdateUserAboutPage(botClient, userContext, _sendMessages, UserActionContextModel),
                new UpdateUserLinksPage(botClient, userContext, _sendMessages, UserActionContextModel),
                new UpdateIsRedyToWorkPage(botClient, userContext, _sendMessages, UserActionContextModel),
                new UpdateUserImagesPage(botClient, userContext, _sendMessages, UserActionContextModel),
                new UpdateUserComplitePage(botClient, userContext, _sendMessages, userService, UserActionContextModel), //5
                new UpdateSeccessUserPage(botClient, userContext, userService, _sendMessages), //6
            };

            InitPagesCatalog(PagesCatalog);
            InitPages(new List<IPage>
            {
                PagesCatalog[5],
                PagesCatalog[6]
            });

        }
    }
}
