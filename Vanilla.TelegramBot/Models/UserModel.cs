namespace Vanilla.TelegramBot.Models
{
    public class UserModel
    {
        public required Guid UserId { get; set; }
        public required string Token { get; set; }
        public required long TelegramId { get; set; }
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Nickname { get; set; }
        public string? About { get; set; }
        public List<string>? Links { get; set; }
        public bool IsRadyForOrders { get; set; }
        public string? LanguageCode { get; set; }
        public required DateTime RegisterInSystemAt { get; set; }
        public required DateTime RegisterInServiceAt { get; set; }

    }
}
