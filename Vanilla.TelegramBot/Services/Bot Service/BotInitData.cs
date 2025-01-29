namespace Vanilla.TelegramBot.Services.Bot_Service
{
    public class BotInitData
    {
        public required long BotId { get; init; }
        public required string FirstName { get; init; }
        public required string Username { get; init; }
        public List<long> Administrations { get; init; } = new List<long>();
        DateTime BotStartDate = DateTime.UtcNow;
        public DateTime StartDate { get => BotStartDate; }
        public string UrlToBot => $"https://t.me/{Username}";
        public required string Environment { get; init; }
        public string? SiteUrl { get; init; }
    }
}
