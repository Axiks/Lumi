using Telegram.BotAPI;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages.UpdateUser;

namespace Vanilla.TelegramBot.Pages
{
    public class UserUpdateProfileFolder : AbstractFolder, IFolder
    {
        public UserUpdateProfileFolder(TelegramBotClient botClient, UserContextModel userContext, IUserService userService, ILogger logger) : base(botClient, userContext, userService, logger)
        {
            var userModel = userContext.UpdateUserContext;

            var PagesCatalog = new List<IPage>
            {
                new UpdateUserNicknamePage(botClient, userContext, this._sendMessages),
                new UpdateUserAboutPage(botClient, userContext, _sendMessages),
                new UpdateUserLinksPage(botClient, userContext, _sendMessages),
                new UpdateIsRedyToWorkPage(botClient, userContext, _sendMessages),
                new UpdateUserImagesPage(botClient, userContext, _sendMessages),
                new UpdateUserComplitePage(botClient, userContext, _sendMessages, userService),
                new UpdateSeccessUserPage(botClient, userContext, _sendMessages),
            };

            this.InitPagesCatalog(PagesCatalog);
            this.InitPages(new List<IPage>
            {
                PagesCatalog[5],
                PagesCatalog[6]
            });

        }
    }
}
