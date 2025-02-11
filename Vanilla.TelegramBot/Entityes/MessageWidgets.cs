﻿using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Helpers;
using Vanilla.TelegramBot.Models;
using Vanilla_App.Services.Projects;

namespace Vanilla.TelegramBot.Entityes
{
    public static class MessageWidgets
    {
        public static string AboutProject(ProjectModel project, Models.UserModel user, UserContextModel userContext)
        {
            string links = "";
            foreach (var link in project.Links)
            {
                Uri linkUri = new Uri(link);
                //links += "<a href=\"" + FavionParser(linkUri.OriginalString) + "\">" + linkUri.Host.ToString()  + "</a>" + "\n";
                links += String.Format("<a href=\"{0}\">&#128279 {1}</a>", linkUri.OriginalString, linkUri.Host.ToString());
                links += " ";
            }

            var username = user.Username is not null ? "@" + user.Username : user.FirstName;
            string developStatusEmoji = FormationHelper.GetEmojiStatus(project.DevelopmentStatus);


            var messageContent = string.Format("<b>{0}</b> \n\n{1} \n\n{2}\n{3} {4}",
                                        project.Name,
                                        project.Description,

                                        links,
                                        developStatusEmoji + " <i>" + userContext.ResourceManager.GetString(project.DevelopmentStatus.ToString()) + "</i>",
                                        username
                                );
            return messageContent;
        }

        public static string AboutProject(BotCreateProjectModel project, Models.UserModel user, UserContextModel userContext)
        {
            string links = "";
            if(project.Links is not null)
            {
                foreach (var link in project.Links)
                {
                    Uri linkUri = new Uri(link);
                    //links += "<a href=\"" + FavionParser(linkUri.OriginalString) + "\">" + linkUri.Host.ToString()  + "</a>" + "\n";
                    links += String.Format("<a href=\"{0}\">&#128279 {1}</a>", linkUri.OriginalString, linkUri.Host.ToString());
                    links += " ";
                }
            }

            var username = user.Username is not null ? "@" + user.Username : user.FirstName;
            string developStatusEmoji = FormationHelper.GetEmojiStatus(project.DevelopmentStatus ?? DevelopmentStatusEnum.Abandoned); // with fix


            var messageContent = string.Format("<b>{0}</b> \n\n{1} \n\n{2}\n{3} {4}",
                                        project.Name,
                                        project.Description,

                                        links,
                                        developStatusEmoji + " <i>" + userContext.ResourceManager.GetString(project.DevelopmentStatus.ToString()) + "</i>",
                                        username
                                );
            return messageContent;
        }

        public static string AboutUser(Models.UserModel user)
        {
            string links = "";
            if (user.Links is not null)
            {
                foreach (var link in user.Links)
                {
                    Uri linkUri = new Uri(link);
                    //links += "<a href=\"" + FavionParser(linkUri.OriginalString) + "\">" + linkUri.Host.ToString()  + "</a>" + "\n";
                    links += String.Format("<a href=\"{0}\">&#128279 {1}</a>", linkUri.OriginalString, linkUri.Host.ToString());
                    links += " ";
                }
            }

            return string.Format("<b>{0}</b> \n\n{1} \n\n{2}\nRady to new job: {3}", user.Nickname, user.About, links, user.IsRadyForOrders);
        }

        public static SendPollArgs GeneratePull(long chatId, UserContextModel userContext)
        {
            var pullOptions = new List<InputPollOption>
                {
                    new InputPollOption(FormationHelper.GetEmojiStatus(DevelopmentStatusEnum.InDevelopment) + " " +  userContext.ResourceManager.GetString(DevelopmentStatusEnum.InDevelopment.ToString())),
                    new InputPollOption(FormationHelper.GetEmojiStatus(DevelopmentStatusEnum.Developed) + " " + userContext.ResourceManager.GetString(DevelopmentStatusEnum.Developed.ToString())),
                    new InputPollOption(FormationHelper.GetEmojiStatus(DevelopmentStatusEnum.PlannedToDevelop) + " " + userContext.ResourceManager.GetString(DevelopmentStatusEnum.PlannedToDevelop.ToString())),
                    new InputPollOption(FormationHelper.GetEmojiStatus(DevelopmentStatusEnum.Abandoned) + " " +  userContext.ResourceManager.GetString(DevelopmentStatusEnum.Abandoned.ToString())),
                    new InputPollOption(FormationHelper.GetEmojiStatus(DevelopmentStatusEnum.Frozen) + " " +  userContext.ResourceManager.GetString(DevelopmentStatusEnum.Frozen.ToString()))
                };

            var pollParameters = new SendPollArgs(chatId, userContext.ResourceManager.GetString("PoolDescription"), pullOptions);
            pollParameters.IsAnonymous = false;

            return pollParameters;
        }

        private static string FavionParser(string url)
        {
            //url = "https://github.com/favicon.ico";
            var iconUrl = String.Format("https://t2.gstatic.com/faviconV2?client=SOCIAL&type=FAVICON&fallback_opts=TYPE,SIZE,URL&url={0}&size=64", url);
            return iconUrl;
        }
    }
}
