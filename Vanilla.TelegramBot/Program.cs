using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla.OAuth.Services;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Repositories;
using Vanilla.TelegramBot.Services;
using Vanilla.TelegramBot.Services.Bot;

namespace Vanilla.TelegramBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello, Vanilla TG bot server");

            // Build a config object, using env vars and JSON providers.
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            // Get values from the config given their key and their target type.
            var settings = config.GetRequiredSection("Settings").Get<SettingsModel>();
            if (settings == null) throw new Exception("No found setting section");

            var services = new ServiceCollection();
            services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseNpgsql(settings.DatabaseConfiguration.ConnectionString),
                ServiceLifetime.Transient);

            services.AddTransient<IUserRepository, Repositories.UserRepository>();
            services.AddTransient<IUserService, Services.UserService>();
            services.AddSingleton<IBotService, BotService>();
            services.AddTransient<ILogger, ConsoleLogger>();

            services.AddTransient<Vanilla.OAuth.Services.UserRepository>();
            services.AddTransient<AuthService>(provider => new AuthService(settings.TokenConfiguration));
            services.AddDbContextFactory<Vanilla.OAuth.ApplicationDbContext>(options =>
               options.UseNpgsql(settings.OAuthDatabaseConfiguration.ConnectionString),
               ServiceLifetime.Transient);

            services.AddDbContextFactory<Vanilla.OAuth.ApplicationDbContext>(options =>
               options.UseNpgsql(settings.OAuthDatabaseConfiguration.ConnectionString),
               ServiceLifetime.Transient);

            services.AddDbContextFactory<Vanilla.Data.ApplicationDbContext>(options =>
               options.UseNpgsql(settings.CoreDatabaseConfiguration.ConnectionString),
               ServiceLifetime.Transient);

            services.AddTransient<Vanilla_App.Interfaces.IUserRepository, Vanilla_App.Repository.UserRepository>();
            services.AddTransient<Vanilla_App.Interfaces.IProjectRepository, Vanilla_App.Repository.ProjectRepository>();
            services.AddTransient<Vanilla_App.Services.UserService>();
            services.AddTransient<Vanilla_App.Interfaces.IProjectService, Vanilla_App.Services.ProjectService>();

            var serviceProvider = services.BuildServiceProvider();

            using (var dbContext = serviceProvider.GetService<ApplicationDbContext>())
            {
                dbContext.Database.EnsureCreated();
                dbContext.Database.Migrate();
                //dbContext.Database.Aut
            }

            using (var dbContext = serviceProvider.GetService<Vanilla.OAuth.ApplicationDbContext>())
            {
                dbContext.Database.EnsureCreated();
                dbContext.Database.Migrate();
            }

            using (var dbContext = serviceProvider.GetService<Vanilla.Data.ApplicationDbContext>())
            {
                dbContext.Database.EnsureCreated();
                dbContext.Database.Migrate();
            }

            RunBot();

            void RunBot()
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
                    RunBot();
                }
            }

        }


 
    }
}
