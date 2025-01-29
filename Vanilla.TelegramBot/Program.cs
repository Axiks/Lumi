using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenTelemetry.Resources;
using Vanilla.Aspire.ServiceDefaults;
using Vanilla.Common.Models;
using Vanilla.OAuth.Services;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Message_Broker;
using Vanilla.TelegramBot.Services;
using Vanilla_App.Module;
using Vanilla_App.Services.Bonus;
using Vanilla_App.Services.Projects;
using Vanilla_App.Services.Projects.Repository;
using Serilog;

namespace Vanilla.TelegramBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello, Lumi TG bot server");

            var builder = WebApplication.CreateBuilder(args);
            // Add service defaults & Aspire components.
            builder.AddServiceDefaults();

            //builder.Host.UseSerilog();

            //builder.WebHost.UseUrls("http://localhost:5008");

            var services = builder.Services;

            var connectionStringTgBotDb = builder.Configuration.GetConnectionString("tgbotdb");
            services.AddDbContextFactory<ApplicationDbContext>(options =>
               options.UseNpgsql(connectionStringTgBotDb),
               ServiceLifetime.Transient);

            services.AddTransient<StorageModule>();
            services.AddTransient<Interfaces.IUserRepository, Repositories.UserRepository>();
            services.AddTransient<IBonusService, BonusService>();
            services.AddTransient<IUserService, Services.UserService>();

            services.AddSingleton<IBotService, BotService>();

            services.AddTransient<Vanilla.TelegramBot.Interfaces.ILogger, ConsoleLoggerService>();

            var connectionStringOAuthDb = builder.Configuration.GetConnectionString("oauthdb");
            services.AddDbContextFactory<Vanilla.OAuth.ApplicationDbContext>(options =>
               options.UseNpgsql(connectionStringOAuthDb),
               ServiceLifetime.Transient);
            services.AddTransient<Vanilla.OAuth.Services.UserRepository>();
            var serviceProvider = services.BuildServiceProvider();
            var oauthRepository = serviceProvider.GetService<Vanilla.OAuth.Services.UserRepository>();

            var tokenPrivateKey = builder.Configuration.GetValue<string>("tokenPrivateKey");
            var tokenLifetimeSec = builder.Configuration.GetValue<int>("tokenLifetimeSec");
            var tokenIssuer = builder.Configuration.GetValue<string>("tokenIssuer");
            var tokenAudience = builder.Configuration.GetValue<string>("tokenAudience");
            var tokenConfig = new TokenConfiguration {
                PrivateKey = tokenPrivateKey,
                LifetimeSec = tokenLifetimeSec,
                Issuer = tokenIssuer,
                Audience = tokenAudience
            };
            services.AddTransient<AuthService>(provider => new AuthService(tokenConfig, oauthRepository));

            services.AddDbContextFactory<Vanilla.OAuth.ApplicationDbContext>(options =>
               options.UseNpgsql(connectionStringOAuthDb),
               ServiceLifetime.Transient);


            var connectionStringCoreDb = builder.Configuration.GetConnectionString("coredb");
            services.AddDbContextFactory<Vanilla.Data.ApplicationDbContext>(options =>
               options.UseNpgsql(connectionStringCoreDb),
               ServiceLifetime.Transient);


            services.AddTransient<Vanilla_App.Services.Users.Repository.IUserRepository, Vanilla_App.Services.Users.Repository.UserRepository>();
            services.AddTransient<IProjectRepository, ProjectRepository>();
            services.AddTransient<Vanilla_App.Services.UserService>();
            services.AddTransient<IProjectService, ProjectService>();

            serviceProvider = services.BuildServiceProvider();
            PrepareDB(serviceProvider);


            builder.Services.AddMassTransit(x =>
            {

                x.AddConsumer<MessageConsumer>();
                x.AddConsumer<TgMessageConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.ReceiveEndpoint("tg-user", e =>
                    {
                        e.ConfigureConsumer<MessageConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("tg-user-2", e =>
                    {
                        e.ConfigureConsumer<TgMessageConsumer>(context);
                    });

                    var connectionStringRQ = builder.Configuration.GetConnectionString("lumi-mq");

                    cfg.Host(connectionStringRQ);
                });
            });


            //builder.Services.AddOpenTelemetry()
            //    .ConfigureResource(resource => resource.AddService(DiagnosticsConfig.ServiceName))
            //    .WithMetrics(metrics =>
            //    {
            //        metrics.AddMeter(DiagnosticsConfig.Meter.Name);
            //    });

            var app = builder.Build();
            //if (!app.Environment.IsDevelopment())
            //{
            //    app.UseExceptionHandler("/Error", createScopeForErrors: true);
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}
            //app.UseHttpsRedirection();
            app.MapDefaultEndpoints();

            app.RunAsync();

            new Thread(delegate ()
            {
                RunBotWatchdog();
            }).Start();

            void RunBotWatchdog()
            {
                var botService = serviceProvider.GetService<IBotService>();
                var logger = serviceProvider.GetService<Vanilla.TelegramBot.Interfaces.ILogger>();
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


        public static void PrepareDB(ServiceProvider serviceProvider)
        {
            using (var dbContext = serviceProvider.GetService<ApplicationDbContext>())
            {
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

    class OrderCreatedConsumer : IConsumer<MessageConsumer>
    {
        public async Task Consume(ConsumeContext<MessageConsumer> context)
        {
            var jsonMessage = JsonConvert.SerializeObject(context.Message);
            Console.WriteLine($"OrderCreated message: {jsonMessage}");
        }
    }
}
