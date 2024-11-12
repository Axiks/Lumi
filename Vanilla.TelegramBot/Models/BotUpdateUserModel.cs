using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla.TelegramBot.Models
{
    public class BotUpdateUserModel : BasicFolderModel
    {
        public String? Nickname { get; set; }
        public String? About { get; set; }
        public List<string>? Links { get; set; }
        public List<ImageModel>? Images { get; set; }
        public bool? IsRadyForOrders { get; set; }
        public bool IsHasProfile { get; set; }

        //General
        public List<int> SendedMessages { get; set; } = new List<int>();
    }
}
