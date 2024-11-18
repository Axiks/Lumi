
using Telegram.BotAPI;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages.Projects.Create;
using Vanilla_App.Interfaces;

namespace Vanilla.TelegramBot.Pages
{
    internal class CreateProjectFolder : AbstractFolder, IFolder
    {
        public CreateProjectFolder(TelegramBotClient botClient, UserContextModel userContext, IUserService userService, ILogger logger, bool isWithCannelButton, IProjectService projectService) : base(botClient, userContext, userService, logger, isWithCannelButton)
        {
            var user = userContext.User;
            var projectModel = new BotCreateProjectModel(user.UserId, user.TelegramId);

            var PagesCatalog = new List<IPage>
            {
                new CreateProjectTitlePage(botClient, userContext, this._sendedMessages, projectModel),
                new CreateProjectDescriptionPage(botClient, userContext, this._sendedMessages, projectModel),
                new CreateProjectPoolPage(botClient, userContext, this._sendedMessages, projectModel),
                new CreateProjectLinksPage(botClient, userContext, this._sendedMessages, projectModel),
                new CreateProjectCompilePage(botClient, userContext, this._sendedMessages, projectModel, projectService),
            };

            this.InitPagesCatalog(PagesCatalog);
            this.InitPages(PagesCatalog);
        }
    }
}
