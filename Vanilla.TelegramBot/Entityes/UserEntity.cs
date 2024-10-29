using System.ComponentModel.DataAnnotations;

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
        public string? LanguageCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
