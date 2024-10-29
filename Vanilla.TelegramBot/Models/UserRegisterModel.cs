namespace Vanilla.TelegramBot.Models
{
    public class UserRegisterModel
    {
        public required long TelegramId { get; set; }
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? LanguageCode { get; set; }
    }
}
