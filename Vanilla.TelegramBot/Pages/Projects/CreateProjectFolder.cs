
using Telegram.BotAPI;
using Vanilla.TelegramBot.Abstract;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages.Projects.Create;
using Vanilla_App.Interfaces;

namespace Vanilla.TelegramBot.Pages.Projects
{
    internal class CreateProjectFolder : AbstractFolder, IFolder
    {
        const string? folderName = "Креатор проекту";
        public CreateProjectFolder(TelegramBotClient botClient, UserContextModel userContext, IUserService userService, ILogger logger, bool isWithCannelButton, IProjectService projectService) : base(botClient, userContext, userService, logger, isWithCannelButton, folderName: folderName)
        {
            var user = userContext.User;
            var projectModel = new BotCreateProjectModel(user.UserId, user.TelegramId);

            var PagesCatalog = new List<IPage>
            {
                new CreateProjectTitlePage(botClient, userContext, _sendedMessages, projectModel),
                new CreateProjectDescriptionPage(botClient, userContext, _sendedMessages, projectModel),
                new CreateProjectPoolPage(botClient, userContext, _sendedMessages, projectModel),
                new CreateProjectLinksPage(botClient, userContext, _sendedMessages, projectModel),
                new CreateProjectCompilePage(botClient, userContext, _sendedMessages, projectModel, projectService),
            };

            InitPagesCatalog(PagesCatalog);
            InitPages(PagesCatalog);
        }
    }
}
