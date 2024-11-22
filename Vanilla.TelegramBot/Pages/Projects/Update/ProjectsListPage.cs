using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.UpdatingMessages;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Abstract;
using Vanilla.TelegramBot.Entityes;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages.Projects.Create;
using Vanilla.TelegramBot.UI;
using Vanilla_App.Interfaces;
using Vanilla_App.Models;
using static Vanilla.TelegramBot.Abstract.ActionFrame;

namespace Vanilla.TelegramBot.Pages.Projects.Update
{
    public class ProjectsListPage : BasicPageAbstract, IPage
    {
        readonly string _deliver = " mya~ ";


        TelegramBotClient _botClient;
        UserContextModel _userContext;
        List<SendedMessageModel> _sendedMessages;
        IProjectService _projectService;

        int? updatedProjectMessageId;
        ProjectUpdateRequestModel? updatedProject;

        int? pinedMessageId;

        public ProjectsListPage(TelegramBotClient botClient, UserContextModel userContext, List<SendedMessageModel> sendedMessages, IProjectService projectService) : base(botClient, userContext, sendedMessages)
        {
            _botClient = botClient;
            _userContext = userContext;
            _sendedMessages = sendedMessages;
            _projectService = projectService;
        }

        public override void InitMessage()
        {
            if (updatedProject is not null) UpdateProjectData();
            else PrintAllUserProjects();
        }

        public override void InitActions()
        {
            var deleteBtnAction = new ActionFrame
            {
                ActionObj = new ActionDelegate(ConfirmDeleteBtnAction),
                Validate = new ValidateDelegate(ValidateDeleteBtnAction),
                Trigger = () => ConfirmDeleteBtnInputTrigger(),
            };

            var updateBtnAction = new ActionFrame
            {
                ActionObj = new ActionDelegate(ConfirmUpdateBtnAction),
                Validate = new ValidateDelegate(ValidateUpdateBtnAction),
                Trigger = () => ConfirmUpdateBtnInputTrigger(),
            };

            var updateTitleBtnAction = new ActionFrame
            {
                ActionObj = new ActionDelegate(UpdateTitleBtnAction),
                Trigger = () => ConfirmUpdateTitleBtnInputTrigger(),
            };

            var updateDescriptionBtnAction = new ActionFrame
            {
                ActionObj = new ActionDelegate(UpdateDescriptionBtnAction),
                Trigger = () => ConfirmUpdateDescriptionBtnInputTrigger(),
            };

            var updateStatusBtnAction = new ActionFrame
            {
                ActionObj = new ActionDelegate(UpdateStatusBtnAction),
                Trigger = () => ConfirmUpdateStatusBtnInputTrigger(),
            };

            var updateLinksBtnAction = new ActionFrame
            {
                ActionObj = new ActionDelegate(UpdateLinksBtnAction),
                Trigger = () => ConfirmUpdateLinksBtnInputTrigger(),
            };


            AddAction(new List<ActionFrame> {
                deleteBtnAction,
                updateBtnAction,
                updateTitleBtnAction,
                updateDescriptionBtnAction,
                updateStatusBtnAction,
                updateLinksBtnAction,
            });
        }

        bool ConfirmDeleteBtnInputTrigger() => ValidateDeleteBtnAction(CurrentUpdate);
        bool ValidateDeleteBtnAction(Telegram.BotAPI.GettingUpdates.Update update)
        {
            if (update.CallbackQuery is null) return false;
            if (update.CallbackQuery.Data is null) return false;
            if (update.CallbackQuery.Data.Length <= 0) return false;

            var spliter = update.CallbackQuery.Data.Split(_deliver);
            if (spliter.Length != 2) return false;
            if (spliter.First() != "delete") return false;

            return true;
        }

        void ConfirmDeleteBtnAction(Telegram.BotAPI.GettingUpdates.Update update)
        {
            var spliter = update.CallbackQuery.Data.Split(_deliver);
            var rawProjectId = spliter.Last();

            Guid projectId = Guid.Parse(rawProjectId);

            ProjectModel project = _projectService.ProjectGetAsync(projectId).Result;

            DeleteProject(project);

            var messageContent = _botClient.EditMessageText(ChatId, update.CallbackQuery.Message.MessageId, text: _userContext.ResourceManager.GetString("ProjectHasBeenDeletedMes"));

            //NextPage();
        }

        void DeleteProject(ProjectModel projecModel)
        {
            if (ItsUserProject(projecModel) == false) throw new Exception("It is`t user project");

            _projectService.ProjectDelete(projecModel.Id);
        }

        bool ConfirmUpdateBtnInputTrigger() => ValidateUpdateBtnAction(CurrentUpdate);
        bool ValidateUpdateBtnAction(Telegram.BotAPI.GettingUpdates.Update update)
        {
            if (update.CallbackQuery is null) return false;
            if (update.CallbackQuery.Data is null) return false;
            if (update.CallbackQuery.Data.Length <= 0) return false;

            var spliter = update.CallbackQuery.Data.Split(_deliver);
            if (spliter.Length != 2) return false;
            if (spliter.First() != "update") return false;
            if (spliter.First().Split(" ").Length != 1) return false;

            return true;
        }

        void ConfirmUpdateBtnAction(Telegram.BotAPI.GettingUpdates.Update update)
        {
            var rawProjectId = update.CallbackQuery.Data.Split(_deliver).Last();
            var projectId = Guid.Parse(rawProjectId);
            
            _botClient.EditMessageReplyMarkup(chatId: ChatId, messageId: update.CallbackQuery.Message.MessageId, replyMarkup: Keyboards.GetProjectUpdateItemsKeyboard(_userContext, projectId));

            /*var projectId = Guid.Parse(update.CallbackQuery.Data.Split(_deliver).Last());
            ProjectModel project = _projectService.ProjectGetAsync(projectId).Result;

            ChangeMessageDeleteMethod(update.CallbackQuery.Message.MessageId, DeleteMessageMethodEnum.NextMessage);

            ChangeFlow(new List<IPage>
            {
                new TESTProjectInfoPage(_botClient, _userContext, _sendedMessages, project, update.CallbackQuery.Message.MessageId),
            });*/
        }

        bool ItsUserProject(ProjectModel projecModel) => projecModel.OwnerId == _userContext.User.UserId;

        void PrintAllUserProjects()
        {
            var userProjects = _projectService.ProjectGetAllAsync().Result.Where(x => x.OwnerId == _userContext.User.UserId).OrderBy(x => x.Created);
            if (userProjects == null || userProjects.Count() == 0) {
                PrintUserDontHaveProjects();
                return;
            }

            foreach (var project in userProjects) {
                var replyMarkuppp = Keyboards.GetProjectInlineKeyboard(_userContext, project.Id);

                var messageObj = _botClient.SendMessage(_userContext.User.TelegramId, MessageWidgets.AboutProject(project, _userContext.User, _userContext),
                    replyMarkup: replyMarkuppp, parseMode: "HTML");

                AddMessage(messageObj.MessageId, DeleteMessageMethodEnum.ExitFolder);
            }
        }

        void PrintUserDontHaveProjects()
        {
            var mess = _botClient.SendMessage(_userContext.User.TelegramId, _userContext.ResourceManager.GetString("UserDontHaveProjectsMess"), replyMarkup: Keyboards.GetCreateProjectInlineKeyboard(_userContext));
            AddMessage(mess.MessageId, DeleteMessageMethodEnum.ClosePage);
        }




        bool ConfirmUpdateTitleBtnInputTrigger() => CallbackBtnActionValidate(CurrentUpdate, "name");
        bool ConfirmUpdateDescriptionBtnInputTrigger() => CallbackBtnActionValidate(CurrentUpdate, "description");
        bool ConfirmUpdateStatusBtnInputTrigger() => CallbackBtnActionValidate(CurrentUpdate, "status");
        bool ConfirmUpdateLinksBtnInputTrigger() => CallbackBtnActionValidate(CurrentUpdate, "links");

        void UpdateTitleBtnAction(Telegram.BotAPI.GettingUpdates.Update update)
        {
            var rawProjectId = update.CallbackQuery.Data.Split(_deliver).Last();
            var projectId = Guid.Parse(rawProjectId);

            updatedProjectMessageId = update.CallbackQuery.Message.MessageId;
            _botClient.PinChatMessage(_userContext.User.TelegramId, (int)updatedProjectMessageId);


            updatedProject = CreateProjectUpdateModel(projectId);

            ChangeFlow(new List<IPage> { new CreateProjectTitlePage(_botClient, _userContext, _sendedMessages, projectUpdateModel: updatedProject) });
        }

        void UpdateDescriptionBtnAction(Telegram.BotAPI.GettingUpdates.Update update)
        {
            var rawProjectId = update.CallbackQuery.Data.Split(_deliver).Last();
            var projectId = Guid.Parse(rawProjectId);

            updatedProjectMessageId = update.CallbackQuery.Message.MessageId;
            _botClient.PinChatMessage(_userContext.User.TelegramId, (int)updatedProjectMessageId);

            updatedProject = CreateProjectUpdateModel(projectId);

            ChangeFlow(new List<IPage> { new CreateProjectDescriptionPage(_botClient, _userContext, _sendedMessages, projectUpdateModel: updatedProject) });
        }

        void UpdateStatusBtnAction(Telegram.BotAPI.GettingUpdates.Update update)
        {
            var rawProjectId = update.CallbackQuery.Data.Split(_deliver).Last();
            var projectId = Guid.Parse(rawProjectId);

            updatedProjectMessageId = update.CallbackQuery.Message.MessageId;
            _botClient.PinChatMessage(_userContext.User.TelegramId, (int)updatedProjectMessageId);

            updatedProject = CreateProjectUpdateModel(projectId);

            ChangeFlow(new List<IPage> { new CreateProjectPoolPage(_botClient, _userContext, _sendedMessages, projectUpdateModel: updatedProject) });
        }

        void UpdateLinksBtnAction(Telegram.BotAPI.GettingUpdates.Update update)
        {
            var rawProjectId = update.CallbackQuery.Data.Split(_deliver).Last();
            var projectId = Guid.Parse(rawProjectId);

            updatedProjectMessageId = update.CallbackQuery.Message.MessageId;
            _botClient.PinChatMessage(_userContext.User.TelegramId, (int)updatedProjectMessageId);

            updatedProject = CreateProjectUpdateModel(projectId);

            ChangeFlow(new List<IPage> { new CreateProjectLinksPage(_botClient, _userContext, _sendedMessages, projectUpdateModel: updatedProject) });
        }

        void UpdateProjectData()
        {
            var project = _projectService.ProjectUpdateAsync(updatedProject).Result;

            var projectMessage = MessageWidgets.AboutProject(project, _userContext.User, _userContext) + "\n\nSuccess updated!";

            _botClient.EditMessageText(_userContext.User.TelegramId, messageId: (int)updatedProjectMessageId, text: projectMessage, parseMode: "HTML", replyMarkup: Keyboards.GetProjectInlineKeyboard(_userContext, project.Id));

            if (pinedMessageId is not null) AddMessage((int)pinedMessageId, DeleteMessageMethodEnum.NextMessage);
            _botClient.UnpinAllChatMessagesAsync(_userContext.User.TelegramId);
        }

        ProjectUpdateRequestModel CreateProjectUpdateModel(Guid projectId)
        {
            var project = _projectService.ProjectGetAsync(projectId).Result;

            return new ProjectUpdateRequestModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                ProjectRequest = project.DevelopmentStatus,
                Links = project.Links,
            };
        }

        bool CallbackBtnActionValidate(Telegram.BotAPI.GettingUpdates.Update update, string command)
        {
            if (update.CallbackQuery is null) return false;
            if (update.CallbackQuery.Data is null) return false;
            if (update.CallbackQuery.Data.Length <= 0) return false;

            var spliter = update.CallbackQuery.Data.Split(_deliver);
            if (spliter.Length != 2) return false;
            if (spliter.First() != command) return false;

            return true;
        }
    }
}
