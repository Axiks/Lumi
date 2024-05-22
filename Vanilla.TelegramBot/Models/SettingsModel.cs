using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla.OAuth.Models;

namespace Vanilla.TelegramBot.Models
{
    public class SettingsModel
    {
        public required string BotAccessToken { get; set; }
        public required DatabaseConfigModel DatabaseConfiguration { get; set; }
        public required DatabaseConfigModel OAuthDatabaseConfiguration { get; set; }
        public required DatabaseConfigModel CoreDatabaseConfiguration { get; set; }
        public required TokenConfiguration TokenConfiguration { get; set; }
    }

    public class DatabaseConfigModel
    {
        public required string Host { get; set; }
        public required string Database { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string ConnectionString => string.Format("Host={0};Database={1};Username={2};Password={3}", Host, Database, Username, Password);
    }

}
