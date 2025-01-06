using Markdig;
using MassTransit;
using MassTransit.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using System;
using System.Text;
using Telegram.BotAPI.AvailableTypes;
using Vanilla.Common;
using Vanilla.Common.Models;
using Vanilla.OAuth.Services;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Message_Broker;
using Vanilla.TelegramBot.Services;
using Vanilla_App.Module;
using Vanilla_App.Services.Bonus;
using Vanilla_App.Services.Projects;
using Vanilla_App.Services.Projects.Repository;

namespace Vanilla.TelegramBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello, Vanilla TG bot server");
            /*
                        Testtt.Job();

                        new Thread(delegate () {
                            Testtt.Job();
                        }).Start();*/


            /*            // start the async task on a separate C# thread
                        Task.Run(async () =>
                        {
                            // do some C# async work
                            await Testtt.Job();
                        });

                        Console.ReadLine();*/


            //var outPutDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);

            //string settingPath = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "Vanilla.Common", "appsettings.json");

            // Build a config object, using env vars and JSON providers.
     /*       IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile(settingPath)
                .AddEnvironmentVariables()


                .Build();*/

            // Get values from the config given their key and their target type.
            //var settings = config.GetRequiredSection("Settings").Get<SettingsModel>();
           /* var settings = new ConfigurationMeneger().Settings;
            if (settings == null) throw new Exception("No found setting section");*/

            //var services = new ServiceCollection();
            //var services = PrepareServices(settings);

            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseUrls("http://localhost:5007");


/*            builder.Services.AddMassTransit(x =>
            {
                //x.AddConsumer<MessageConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ReceiveEndpoint("message_queue", e =>
                    {
                        e.ConfigureConsumer<MessageConsumer>(context);
                    });

                });

            });*/

            //var serviceProvider = builder.Services.BuildServiceProvider();
            var services = builder.Services;
            //PrepareDB(serviceProvider);

            var connectionStringTgBotDb = builder.Configuration.GetConnectionString("tgbotdb");
            services.AddDbContextFactory<ApplicationDbContext>(options =>
               options.UseNpgsql(connectionStringTgBotDb),
               ServiceLifetime.Transient);

            services.AddTransient<StorageModule>();
            services.AddTransient<Interfaces.IUserRepository, Repositories.UserRepository>();
            services.AddTransient<IBonusService, BonusService>();
            services.AddTransient<IUserService, Services.UserService>();

            //var botAccessToken = builder.Configuration.GetValue<string>("hostnameRQ");
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

            /* services.AddDbContextFactory<Vanilla.Data.ApplicationDbContext>(options =>
                options.UseNpgsql(ConnectionStringDB(builder.Configuration, settings.CoreDatabaseConfiguration.Database)),
                ServiceLifetime.Transient);

             builder.AddNpgsqlDbContext<Vanilla.Data.ApplicationDbContext>(connectionName: "coredb");

             builder.Services.AddDbContext<ITestDbContext, TestDbContext>(optionsBuilder =>
             {
                 optionsBuilder.UseSqlServer(ConnectionString);
             }, contextLifetime: ServiceLifetime.Singleton);

             builder.EnrichSqlServerDbContext<TestDbContext>();
 */


            services.AddTransient<Vanilla_App.Services.Users.Repository.IUserRepository, Vanilla_App.Services.Users.Repository.UserRepository>();
            services.AddTransient<IProjectRepository, ProjectRepository>();
            services.AddTransient<Vanilla_App.Services.UserService>();
            services.AddTransient<IProjectService, ProjectService>();

            serviceProvider = services.BuildServiceProvider();
            PrepareDB(serviceProvider);

            //builder.AddRabbitMQClient(connectionName: "messaging");

            builder.Services.AddMassTransit(x =>
            {
                //x.AddConsumer<AboutUserConsumer>();
                /*           x.AddConsumer<AboutUserConsumer>()
                                      .Endpoint(e => e.Name = "tg-user");*/

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

                   
/*
                    var hostnameRQ = builder.Configuration.GetValue<string>("hostnameRQ");
                    var usernameRQ = builder.Configuration.GetValue<string>("usernameRQ");
                    var passwordRQ = builder.Configuration.GetValue<string>("passwordRQ");
                    ushort portRQ = builder.Configuration.GetValue<ushort>("portRQ");*/

                    var connectionStringRQ = builder.Configuration.GetConnectionString("lumi-mq");

                    cfg.Host(connectionStringRQ);

              /*     cfg.Host(hostnameRQ, portRQ, "/", h =>
                    {
                        h.Username(usernameRQ);
                        h.Password(passwordRQ);
                        *//*    h.Username(settings.RabitMQConfiguration.Username);
                            h.Password(settings.RabitMQConfiguration.Password);*//*
                    });*/

                    /*                cfg.ReceiveEndpoint("tg-user", e =>
                                    {
                                        e.ConfigureConsumer<MessageConsumer>(context);
                                    });*/

                    //cfg.ConfigureEndpoints(context);
                });

                /*            x.ConfigureHealthCheckOptions(options =>
                            {
                                options.Name = "masstransit";
                                options.MinimalFailureStatus = HealthStatus.Unhealthy;
                                options.Tags.Add("health");
                            });*/
            });
                /*        .AddSingleton<IAsyncBusHandle, AsyncBusHandle>()
                        .RemoveMassTransitHostedService();*/

            //builder.Services.AddScoped<AboutUserConsumer>();



            var app = builder.Build();
            app.RunAsync();
            // Temp api (remove in future)
/*            new Thread(delegate ()
            {
                RunMinimalApi(args, serviceProvider);
            }).Start();*/


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

/*        static string ConnectionStringDB(ConfigurationManager configurationManager, string dbName)
        {
            var host = configurationManager.GetValue<string>("hostDB");
            var username = configurationManager.GetValue<string>("usernameDB");
            var password = configurationManager.GetValue<string>("passwordDB");
            return string.Format("Host={0};Database={1};Username={2};Password={3}", Environment.GetEnvironmentVariable("DB_HOST") ?? host, dbName, username, password);
        }*/


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


/*        async static void RunMinimalApi(string[] args, ServiceProvider serviceProvider)
        {
            var userService = serviceProvider.GetService<IUserService>();
            var projectsService = serviceProvider.GetService<IProjectService>();

            ConfigurationMeneger confManager = new ConfigurationMeneger();
            var _settings = confManager.Settings;

            string PrintProfilesList()
            {
                var users = userService.GetUsers().Result.Where(x => x.IsHasProfile == true);

                var message = "";

                foreach (var user in users) {
                    var profileUrl = String.Format("https://{0}/users/{1}", _settings.Domain, user.UserId.ToString());
                    message += String.Format("**[{0}]({1})** \n --- \n\n", user.Username, profileUrl);
                }

                return Markdown.ToHtml(message);
            }

            string PrintProfileInformation(Guid userId)
            {
                var user = userService.GetUser(userId).Result;

                var userProfile = "![" + user.Username + "](https://" + _settings.Domain + "/storage/" + user.Images.FirstOrDefault().TgMediaId + ".jpg) \r\n\n**" + user.Username + "**\r\n\r\n" + user.About + "\r\n\r\n\r\n";

                var links = String.Format("[{0}](http://t.me/{1})\r\n", user.Username, user.Username);
                if (user.Links is not null)
                {
                    foreach (var link in user.Links)
                    {
                        links += String.Format("[{0}]({1})\r\n", link, link);
                    }
                }
                userProfile += links;

                var userProjects = "\n\n---\r\n";

                var projects = projectsService.ProjectGetAllAsync().Result.Where(x => x.OwnerId == user.UserId);
                if (projects is not null)
                {
                    foreach (var project in projects)
                    {
                        var projectLinks = "";

                        foreach (var projectLink in project.Links)
                        {
                            projectLinks += String.Format("[{0}]({1})\r\n", projectLink, projectLink);
                        }

                        userProjects += String.Format("**{0}**\r\n*{1}*\r\n\r\n{2}\r\n\r\n{3}\r\n\r\n---\n\n", project.Name, project.DevelopmentStatus.ToString(), project.Description, projectLinks);
                    }
                }

                userProfile += userProjects;

                return Markdown.ToHtml(userProfile);
            }


            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();


            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "storage")),
                RequestPath = "/storage"
            });
            app.MapGet("/", () => "Hello World!");
            app.MapGet("/health", () => "online");

            app.MapGet("/users",
                async context =>
                {
                    context.Response.ContentType = "text/html; charset=UTF8";

                    var result = PrintProfilesList();
                    await context.Response.WriteAsync(result, Encoding.UTF8);

                });

            app.MapGet("/users/{userId}", 
                async context =>
            {
                context.Response.ContentType = "text/html; charset=UTF8";

                var someValueFromGet = context.Response;

                if (context.Request.RouteValues.ContainsKey("userId"))
                {
                    var result = PrintProfileInformation(Guid.Parse(context.Request.RouteValues["userId"].ToString()));
                    await context.Response.WriteAsync(result, Encoding.UTF8);
                }

            });


            app.Run();
        }*/



    }

    static class Testtt
    {
        public static async Task Job()
        {
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.ReceiveEndpoint("order-created-event", e =>
                {
                    e.Consumer<OrderCreatedConsumer>();
                });

                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

            });


            await busControl.StartAsync(new CancellationToken());
            try
            {
                Console.WriteLine("Press enter to exit");
                await Task.Run(() => Console.ReadLine());
            }
            finally
            {
                await busControl.StopAsync();
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
