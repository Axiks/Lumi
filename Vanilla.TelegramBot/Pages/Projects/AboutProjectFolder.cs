
using Telegram.BotAPI;
using Vanilla.TelegramBot.Abstract;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages.Projects.Update;
using Vanilla_App.Services.Projects;

namespace Vanilla.TelegramBot.Pages.Projects
{
    internal class AboutProjectFolder : AbstractFolder, IFolder
    {
        const string? folderName = "Проекти";
        public AboutProjectFolder(TelegramBotClient botClient, UserContextModel userContext, IUserService userService, ILogger logger, bool isWithCannelButton, IProjectService projectService) : base(botClient, userContext, userService, logger, isWithCannelButton, folderName: folderName)
        {
            var user = userContext.User;
            //var projectCreateModel = new BotCreateProjectModel(user.UserId, user.TelegramId);
            //var project = projectService.ProjectGetAsync(projectId).Result;
            //var projectUpdateModel = new ProjectUpdateRequestModel { Id = project.Id };

            var PagesCatalog = new List<IPage>
            {
                new ProjectsListPage(botClient, userContext, _sendedMessages, projectService),
                //new ProjectInfoPage(botClient, userContext, this._sendedMessages, project),
                /*new CreateProjectTitlePage(botClient, userContext, this._sendedMessages, projectCreateModel),
                new CreateProjectDescriptionPage(botClient, userContext, this._sendedMessages, projectCreateModel),
                new CreateProjectPoolPage(botClient, userContext, this._sendedMessages, projectCreateModel),
                new CreateProjectLinksPage(botClient, userContext, this._sendedMessages, projectCreateModel),
                new CreateProjectCompilePage(botClient, userContext, this._sendedMessages, projectCreateModel, projectService),*/
            };

            InitPagesCatalog(PagesCatalog);
            InitPages(PagesCatalog);
        }
    }
}
