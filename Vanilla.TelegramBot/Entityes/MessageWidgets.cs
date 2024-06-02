using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Models;
using Vanilla_App.Models;

namespace Vanilla.TelegramBot.Entityes
{
    public static class MessageWidgets
    {
        public static string AboutProject(ProjectModel project, Models.UserModel user)
        {
            string links = "";
            foreach (var link in project.Links)
            {
                Uri linkUri = new Uri(link);
                links += "<a href=\"" + linkUri + "\">" + linkUri.Host.ToString() + "</a>" + "\n";
            }

            var messageContent = string.Format("<b>{0}</b> \n{1} \n\n{2}\n{3}\n{4}",
                                        project.Name,
                                        project.Description,
                                        "<i>" + project.DevelopmentStatus.ToString() + "</i>",
                                        links,
                                        "@" + user.Username
                                );
            return messageContent;
        }

        public static SendPollArgs GeneratePull(long chatId)
        {
            var pullOptions = new List<InputPollOption>
                {
                    new InputPollOption
                    {
                        Text = DevelopmentStatusEnum.InDevelopment.ToString(),
                    },
                    new InputPollOption
                    {
                        Text = DevelopmentStatusEnum.Developed.ToString()
                    },
                    new InputPollOption
                    {
                        Text = DevelopmentStatusEnum.PlannedToDevelop.ToString()
                    },
                    new InputPollOption
                    {
                        Text = DevelopmentStatusEnum.Abandoned.ToString()
                    }
                };

            var pollParameters = new SendPollArgs(chatId, "Select the completion project status", pullOptions);
            pollParameters.IsAnonymous = false;

            return pollParameters;
        }
    }
}
