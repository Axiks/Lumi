namespace Vanilla.TelegramBot.Models
{
    public class UserRegisterModel
    {
        public required long TelegramId { get; set; }
        public string? Username { get; set; }
        public required string Nickname { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? About { get; set; }
        public List<string>? Links { get; set; }
        public bool? IsRadyForOrders { get; set; }
        public string? LanguageCode { get; set; }
    }
}
