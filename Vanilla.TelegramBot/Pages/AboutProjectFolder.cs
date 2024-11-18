
using Telegram.BotAPI;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages.Projects.Create;
using Vanilla.TelegramBot.Pages.Projects.Update;
using Vanilla_App.Interfaces;
using Vanilla_App.Models;

namespace Vanilla.TelegramBot.Pages
{
    internal class AboutProjectFolder : AbstractFolder, IFolder
    {
        public AboutProjectFolder(TelegramBotClient botClient, UserContextModel userContext, IUserService userService, ILogger logger, bool isWithCannelButton, IProjectService projectService) : base(botClient, userContext, userService, logger, isWithCannelButton)
        {
            var user = userContext.User;
            //var projectCreateModel = new BotCreateProjectModel(user.UserId, user.TelegramId);
            //var project = projectService.ProjectGetAsync(projectId).Result;
            //var projectUpdateModel = new ProjectUpdateRequestModel { Id = project.Id };

            var PagesCatalog = new List<IPage>
            {
                new ProjectsListPage(botClient, userContext, this._sendedMessages, projectService),
                //new ProjectInfoPage(botClient, userContext, this._sendedMessages, project),
                /*new CreateProjectTitlePage(botClient, userContext, this._sendedMessages, projectCreateModel),
                new CreateProjectDescriptionPage(botClient, userContext, this._sendedMessages, projectCreateModel),
                new CreateProjectPoolPage(botClient, userContext, this._sendedMessages, projectCreateModel),
                new CreateProjectLinksPage(botClient, userContext, this._sendedMessages, projectCreateModel),
                new CreateProjectCompilePage(botClient, userContext, this._sendedMessages, projectCreateModel, projectService),*/
            };

            this.InitPagesCatalog(PagesCatalog);
            this.InitPages(PagesCatalog);
        }
    }
}
