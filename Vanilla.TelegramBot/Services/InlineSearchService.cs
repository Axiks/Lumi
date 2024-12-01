using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.Extensions;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.InlineMode;
using Vanilla.TelegramBot.Entityes;
using Vanilla.TelegramBot.Helpers;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.UI.Widgets;
using Vanilla_App.Interfaces;
using Vanilla_App.Models;
using static System.Net.Mime.MediaTypeNames;
using UserModel = Vanilla.TelegramBot.Models.UserModel;

namespace Vanilla.TelegramBot.Services
{
    public class InlineSearchService(TelegramBotClient _botClient, IProjectService _projectService, IUserService _userService)
    {
        public void InlineSearch(Update update, UserContextModel? userContext)
        {
            var inline = update.InlineQuery;
            var query = inline.Query;

            var projects = Router(query, userContext); // new
            AnswerInlineQuery(update, projects, userContext);
        }

        /*        void AnswerInlineQuery(Update update, List<ProjectModel> projects, UserContextModel? userContext)
                {
                    var inline = update.InlineQuery;

                    var inlineOffset = inline.Offset;

                    int maxProjects = 48;
                    int index = inlineOffset is not null && inlineOffset != "" ? int.Parse(inlineOffset) + 1 : 0;
                    int toIndex = index + 1 + maxProjects <= projects.Count ? index + 1 + maxProjects : projects.Count;

                    var offsetProjectId = toIndex < projects.Count ? toIndex.ToString() : null;

                    var res = new List<InlineQueryResult>();
                    if (userContext is not null && userContext.User.IsHasProfile && !inline.Query.Contains("@"))
                    {
                        var user = userContext.User;
                        res.Add(GetInlineUserProfile(user, index.ToString()));
                        index++;
                    }
                    else if (inline.Query.Contains("@"))
                    {
                        var username = inline.Query.Substring(1).Split(" ")[0];
                        if (username == "")
                        {
                            var users = _userService.GetUsers().Result;
                            foreach (var user in users)
                            {
                                res.Add(GetInlineUserProfile(user, index.ToString()));
                                index++; // Can be overflow
                            }
                        }
                        else
                        {
                            var users = _userService.FindByUsername(username).Result;
                            if (users.Count > 0)
                            {
                                var user = users.First();
                                res.Add(GetInlineUserProfile(user, index.ToString()));
                                index++;
                            }
                        }

                    }

                    while (index < toIndex)
                    {
                        var project = projects[index];
                        res.Add(GetInlineProjectProfile(project, index.ToString(), userContext));

                        index++;
                    }

                    //var ans = new AnswerInlineQueryArgs(inline.Id, res);

                    var inlineAddOwnProjectButton = new InlineQueryResultsButton
                    {
                        Text = userContext.ResourceManager.GetString("AddOwnProject"),
                        StartParameter = "addProject"
                    };
                    _botClient.AnswerInlineQuery(inlineQueryId: inline.Id, results: res, button: inlineAddOwnProjectButton, nextOffset: offsetProjectId, cacheTime: 24);
                }*/

        void AnswerInlineQuery(Update update, List<ProjectModel> projectsq, UserContextModel? userContext)
        {
            const int maxResults = 48;

            var inline = update.InlineQuery;

            var inlineOffset = inline.Offset;
            int index = inlineOffset is not null && inlineOffset != "" ? int.Parse(inlineOffset) + 1 : 0;

            // Make items list
            var fullResultItemsList = new List<InlineQueryResult>();



            var users = new List<UserModel>();

            if (userContext is not null && userContext.User.IsHasProfile && !inline.Query.Contains("@"))
            {
                // Past current user profile
                var curentUser = userContext.User;
                users.Add(curentUser);
            }
            else if (inline.Query.Contains("@"))
            {
                // Search useres profile
                var username = inline.Query.Substring(1).Split(" ")[0];

                // All profiles
                if (username == "") users.AddRange(_userService.GetUsers().Result.Where(x => x.IsHasProfile == true));
                else
                {
                    // By username profile
                    var usersByUsername = _userService.FindByUsername(username).Result.Where(x => x.IsHasProfile == true).ToList();
                    if (usersByUsername.Count > 0) users.Add(usersByUsername.First());
                }

            }

            // Add all users profiles to top list
            var j = 0;
            foreach (var user in users) {
                fullResultItemsList.Add(GetInlineUserProfile(user, j.ToString(), userContext));
                j++;
            }

            // Add alll projects
            foreach (var project in projectsq)
            {
                fullResultItemsList.Add(GetInlineProjectProfile(project, j.ToString(), userContext));
                j++;
            }



            int toIndex = index + 1 + maxResults <= fullResultItemsList.Count ? index + 1 + maxResults : fullResultItemsList.Count;
            var offsetResId = toIndex < fullResultItemsList.Count ? toIndex.ToString() : null;

            var resultItemsList = new List<InlineQueryResult>();
            while (index < toIndex)
            {
                resultItemsList.Add(fullResultItemsList[index]);

                index++;
            }


            var inlineAddOwnProjectButton = new InlineQueryResultsButton
            {
                Text = userContext.ResourceManager.GetString("AddOwnProject"),
                StartParameter = "addProject"
            };

            var inlineRegistrationButton = new InlineQueryResultsButton
            {
                Text = userContext.ResourceManager.GetString("Registration"),
                //StartParameter = "addProject"
            };

            var inlineButton = userContext is not null && userContext.User.IsHasProfile == true ? inlineAddOwnProjectButton : inlineRegistrationButton;

            _botClient.AnswerInlineQuery(inlineQueryId: inline.Id, results: resultItemsList, button: inlineButton, nextOffset: offsetResId, cacheTime: 24);
        }
        InlineQueryResultArticle GetInlineUserProfile(UserModel user, string ResultId, UserContextModel userContext)
        {
            var name = user.Nickname ?? user.FirstName ?? "UNDEFINED";

            //var messageContent = MessageWidgets.AboutUser(user);
            var messageContent = Widjets.AboutUser(userContext.ResourceManager, user);
            var inputMessage = new InputTextMessageContent(messageContent);

            //var img = user.Images is not null && user.Images.Count() > 0 ? user.Images.First().TgUrl : "https://img.freepik.com/premium-vector/smile-girl-anime-error-404-page-found_150972-827.jpg";
            if (user.Images is not null && user.Images.Count() > 0)
            {
                /*var file = _botClient.GetFile(user.Images.First().TgMediaId);
                var imgUrl = _botClient.BuildFileDownloadLink(file);*/

                var profileImgUrl = "https://dev-lumi.neko3.space/storage/" + user.Images.First().TgMediaId + ".jpg";

                inputMessage.LinkPreviewOptions = new Telegram.BotAPI.AvailableTypes.LinkPreviewOptions
                {
                    PreferLargeMedia = true,
                    ShowAboveText = true,
                    //Url = "https://img.freepik.com/premium-vector/smile-girl-anime-error-404-page-found_150972-827.jpg"
                    Url = profileImgUrl
                };
            }
            inputMessage.ParseMode = "HTML";

            var description = "🐱 @" + user.Username;


            var result = new InlineQueryResultArticle
            {
                Id = ResultId,
                Title = name,
                Description = description,
                InputMessageContent = inputMessage,
                //ThumbnailUrl = "https://avatarfiles.alphacoders.com/293/293990.jpg",
            };

            if (user.Images is not null && user.Images.Count() > 0)
            {
                //var profileImgUrl = "https://avatarfiles.alphacoders.com/293/293990.jpg";
                //var profileImgUrl = "https://dev-lumi.neko3.space/storage/" + user.Images.First().TgMediaId + "_thumbnail.jpg";
                var profileImgUrl = "https://dev-lumi.neko3.space/storage/" + user.Images.First().TgMediaId + ".jpg";
                result.ThumbnailUrl = profileImgUrl;
            }

            return result;

            /*           return new InlineQueryResultCachedPhoto
                       {
                           Id = ResultId,
                           PhotoFileId = user.Images is not null ? user.Images.First().TgMediaId : "AgACAgIAAxkBAAIkc2LB6mYy0GnZBzKTXuxG_2qCaQAB2wACkr4xG8AuEEqrRCYAAUPEFEMBAAMCAANzAAMpBA",
                           Title = "title test",
                           Caption = "messageContent test",
                           ShowCaptionAboveMedia = true,
                       };*/


            /*            return new InlineQueryResultPhoto
                        {
                            Id = ResultId,
                            PhotoUrl = "https://img.freepik.com/premium-vector/smile-girl-anime-error-404-page-found_150972-827.jpg",
                            ThumbnailUrl = "https://avatarfiles.alphacoders.com/293/293990.jpg",
                            Title = name,
                            Description = description,
                            Caption = messageContent,
                            ShowCaptionAboveMedia = true,
                            ParseMode = "HTML",
                        };*/

            /*           return new InlineQueryResultArticle
                       {
                           Id = ResultId,
                           Title = name,
                           Description = description,
                           InputMessageContent = inputMessage,
                       };*/
        }

        InlineQueryResultArticle GetInlineProjectProfile(ProjectModel project, string ResultId, UserContextModel userContextModel)
        {
            var owner = _userService.GetUser(project.OwnerId).Result;
            var ownerName = owner.Username is not null ? "@" + owner.Username : owner.FirstName;

            var developStatusEmoji = FormationHelper.GetEmojiStatus(project.DevelopmentStatus);
            var description = developStatusEmoji + " " + ownerName + "\n" + project.Description;

            int messageMaxLenght = 4090;
            if (description.Length >= messageMaxLenght)
            {
                int howMuchMore = description.Length - messageMaxLenght;
                int abbreviation = description.Length - howMuchMore - 3;
                description = ownerName + "\n" + project.Description.Substring(0, abbreviation) + "...";
            }

            var messageContent = MessageWidgets.AboutProject(project, owner, userContextModel);
            var inputMessage = new InputTextMessageContent(messageContent);
            inputMessage.ParseMode = "HTML";

            return new InlineQueryResultArticle
            {
                Id = ResultId,
                Title = project.Name,
                Description = description,
                InputMessageContent = inputMessage,
            };
        }

        List<ProjectModel> Router(string query, UserContextModel? userContextModel)
        {
            var projects = new List<ProjectModel>();
            if (query is null) return new List<ProjectModel>();

            if (query == "") projects = GetAllProjects();
            else if (query.Contains("@")) projects = SearchProjectsByUsername(query);
            else projects = SearchProjectsByName(query);

            if (userContextModel is not null) projects = UserProjectsToTopHelper(projects, userContextModel.User.UserId);

            return projects;
        }

        List<ProjectModel> UserProjectsToTopHelper(List<ProjectModel> projects, Guid userId)
        {
            var userProjects = new List<ProjectModel>(projects.Where(x => x.OwnerId == userId));
            projects.RemoveAll(x => x.OwnerId == userId);
            projects.InsertRange(0, userProjects);
            return projects;
        }

        List<ProjectModel> GetAllProjects() => _projectService.ProjectGetAllAsync().Result.OrderByDescending(x => x.Created).ToList();

        List<ProjectModel> SearchProjectsByName(string query)
        {
            var allProjects = _projectService.ProjectGetAllAsync().Result.OrderByDescending(x => x.Created).ToList();
            return allProjects.Where(x => x.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        List<ProjectModel> GetUserProjectsByUsername(Guid userId, string? q = null)
        {
            var allProjects = _projectService.ProjectGetAllAsync();
            var searchUserProjects = allProjects.Result.Where(x => x.OwnerId == userId);
            if (q is not null) searchUserProjects = searchUserProjects.Where(x => x.Name.Contains(q, StringComparison.InvariantCultureIgnoreCase));
            return searchUserProjects.ToList();
        }

        List<ProjectModel> SearchProjectsByUsername(string query)
        {
            var username = query.Substring(1).Split(" ")[0];
            var users = _userService.FindByUsername(username).Result;

            var userSerchQuery = query.Split(" ");
            var q = userSerchQuery.Length > 1 ? string.Concat(userSerchQuery.Skip(1).ToArray()) : null;

            List<ProjectModel> projects = new List<ProjectModel>();
            foreach (var user in users)
            {
                var userProjects = GetUserProjectsByUsername(user.UserId, q).OrderByDescending(x => x.Created);
                projects.AddRange(userProjects);
            }

            return projects;
        }

    }
}
