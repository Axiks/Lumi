using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla.TelegramBot.Models
{
    public class UserContextModel
    {
        public readonly UserModel User;

        public BotCreateProjectModel? CreateProjectContext {  get; set; }
        public BotUpdateProjectModel? UpdateProjectContext {  get; set; }

        public UserContextModel(UserModel user)
        {
            User = user;
        }
    }
}
