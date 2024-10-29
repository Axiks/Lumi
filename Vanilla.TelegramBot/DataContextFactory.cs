using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Vanilla.Common;


namespace Vanilla.TelegramBot
{
    public class DataContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {

        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            var settings = new ConfigurationMeneger().Settings;
            var connectionString = settings.TgBotDatabaseConfiguration.ConnectionString;

            optionsBuilder.UseNpgsql(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }

    }
}
