namespace Vanilla.TelegramBot.Services.Bot_Service
{
    public class UpdateUserData
    {
        public long TgId {  get; init; }
        public string? Username {  get; set; }
        public string? Firstname {  get; set; }
        public string? Lastname {  get; set; }
        public string? LanguageCode {  get; set; }
        public bool? IsAdmin { get; set; } = false;
    }
}
