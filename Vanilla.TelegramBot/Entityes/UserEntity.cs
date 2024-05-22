using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla.TelegramBot.Entityes
{
    public class UserEntity
    {
        [Key]
        public required Guid UserId { get; set; }
        public required long TelegramId { get; set; }
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt {  get; set; } = DateTime.UtcNow;
    }
}
