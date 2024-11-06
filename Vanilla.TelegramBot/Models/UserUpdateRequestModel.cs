using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla.TelegramBot.Models
{
    public class UserUpdateRequestModel
    {
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? LanguageCode { get; set; }
        public string? Nickname { get; set; }
        public string? About { get; set; }
        public List<ImageModel>? Images { get; set; }
        public List<string>? Links { get; set; }
        public bool? IsRadyForOrders { get; set; }

    }
}
