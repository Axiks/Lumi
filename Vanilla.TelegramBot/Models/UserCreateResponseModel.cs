namespace Vanilla.TelegramBot.Models
{
    public class UserCreateResponseModel
    {
        public required Guid UserId { get; set; }
        public required long TelegramId { get; set; }
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? LanguageCode { get; set; }
        public List<ImageModel>? Images { get; set; }
        public required DateTime CreatedAt { get; set; }
        public bool IsHasProfile { get; set; }
    }
}
