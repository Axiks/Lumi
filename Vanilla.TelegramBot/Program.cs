using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vanilla.Common;
using Vanilla.OAuth.Services;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Services;
using Vanilla.TelegramBot.Services.Bot;
using Vanilla_App.Interfaces;
using Vanilla_App.Services;

namespace Vanilla.TelegramBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello, Vanilla TG bot server");

            /*            //var outPutDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);

                        string settingPath = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "Vanilla.Common", "appsettings.json");

                        // Build a config object, using env vars and JSON providers.
                        IConfigurationRoot config = new ConfigurationBuilder()
                            .AddJsonFile(settingPath)
                            .AddEnvironmentVariables()
                            .Build();*/

            // Get values from the config given their key and their target type.
            //var settings = config.GetRequiredSection("Settings").Get<SettingsModel>();
            var settings = new ConfigurationMeneger().Settings;
            if (settings == null) throw new Exception("No found setting section");

            //var services = new ServiceCollection();
            var services = PrepareServices(settings);

            var serviceProvider = services.BuildServiceProvider();
            PrepareDB(serviceProvider);

            RunBotWatchdog();
            void RunBotWatchdog()
            {
                var botService = serviceProvider.GetService<IBotService>();
                var logger = serviceProvider.GetService<ILogger>();
                try
                {
                    var x = botService.StartListening();
                    x.Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    logger.WriteLog(ex.Message, Common.Enums.LogType.Error);

                    int i = 1;
                    while (i < 30)
                    {
                        Thread.Sleep(1000);
                        Console.WriteLine("Sleep sec: " + i.ToString());
                        i++;
                    }
                    RunBotWatchdog();
                }
            }

        }

        public static ServiceCollection PrepareServices(SettingsModel settings)
        {
            var services = new ServiceCollection();

            services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseNpgsql(settings.TgBotDatabaseConfiguration.ConnectionString),
                ServiceLifetime.Transient);

            services.AddTransient<Interfaces.IUserRepository, Repositories.UserRepository>();
            services.AddTransient<IBonusService, BonusService>();
            services.AddTransient<IUserService, Services.UserService>();
            services.AddSingleton<IBotService, BotService>();
            services.AddTransient<ILogger, ConsoleLogger>();

            services.AddTransient<Vanilla.OAuth.Services.UserRepository>();
            services.AddTransient<AuthService>(provider => new AuthService(settings.TokenConfiguration));

            services.AddDbContextFactory<Vanilla.OAuth.ApplicationDbContext>(options =>
               options.UseNpgsql(settings.OAuthDatabaseConfiguration.ConnectionString),
               ServiceLifetime.Transient);

            /*  services.AddDbContextFactory<Vanilla.OAuth.ApplicationDbContext>(options =>
                 options.UseNpgsql(settings.OAuthDatabaseConfiguration.ConnectionString),
                 ServiceLifetime.Transient);*/

            services.AddDbContextFactory<Vanilla.Data.ApplicationDbContext>(options =>
               options.UseNpgsql(settings.CoreDatabaseConfiguration.ConnectionString),
               ServiceLifetime.Transient);

            services.AddTransient<Vanilla_App.Interfaces.IUserRepository, Vanilla_App.Repository.UserRepository>();
            services.AddTransient<Vanilla_App.Interfaces.IProjectRepository, Vanilla_App.Repository.ProjectRepository>();
            services.AddTransient<Vanilla_App.Services.UserService>();
            services.AddTransient<Vanilla_App.Interfaces.IProjectService, Vanilla_App.Services.ProjectService>();

            return services;
        }

        public static void PrepareDB(ServiceProvider serviceProvider)
        {
            using (var dbContext = serviceProvider.GetService<ApplicationDbContext>())
            {
                //dbContext.Database.EnsureCreated();
                dbContext.Database.Migrate();
            }

            using (var dbContext = serviceProvider.GetService<Vanilla.OAuth.ApplicationDbContext>())
            {
                dbContext.Database.Migrate();
            }

            using (var dbContext = serviceProvider.GetService<Vanilla.Data.ApplicationDbContext>())
            {
                dbContext.Database.Migrate();
            }
        }

    }
}
