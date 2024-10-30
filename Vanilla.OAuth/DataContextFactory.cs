using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Vanilla.Common;


namespace Vanilla.OAuth
{
    public class DataContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {

        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            var settings = new ConfigurationMeneger().Settings;
            var connectionString = settings.OAuthDatabaseConfiguration.ConnectionString;

            optionsBuilder.UseNpgsql(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }

    }
}
