using Vanilla.Common.Models;

namespace Vanilla.Common
{
    public class SettingsModel
    {
        public required string BotAccessToken { get; set; }
        public required DatabaseConfigModel TgBotDatabaseConfiguration { get; set; }
        public required DatabaseConfigModel OAuthDatabaseConfiguration { get; set; }
        public required DatabaseConfigModel CoreDatabaseConfiguration { get; set; }
        public required TokenConfiguration TokenConfiguration { get; set; }
    }

    public class DatabaseConfigModel
    {
        public required string Host { get; init; }
        public required string Database { get; init; }
        public required string Username { get; init; }
        public required string Password { get; init; }
        public string ConnectionString => string.Format("Host={0};Database={1};Username={2};Password={3}", Environment.GetEnvironmentVariable("DB_HOST") ?? Host, Database, Username, Password);
    }

}
