using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla.TelegramBot.Models
{
    public class UserCreateResponseModel
    {
        public required Guid UserId { get; set; }
        public required long TelegramId { get; set; }
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
