using Markdig;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Text;
using Vanilla.Aspire.ServiceDefaults;
using Vanilla.Common;
using Vanilla.Data;
using Vanilla_App.Services;
using Vanilla_App.Services.Projects;
using Vanilla_App.Services.Projects.Repository;
using Vanilla_App.Services.Users.Repository;
using MassTransit;
using Vanilla.Common.Message_Broker;
using Vanilla_App.Services.Users;
using Vanilla_App.Module;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

var settings = new ConfigurationMeneger().Settings;
if (settings == null) throw new Exception("No found setting section");

builder.Services.AddTransient<StorageModule>();
builder.Services.AddTransient<Vanilla.OAuth.Services.UserRepository>();

builder.Services.AddDbContextFactory<Vanilla.OAuth.ApplicationDbContext>(options =>
   options.UseNpgsql(settings.OAuthDatabaseConfiguration.ConnectionString),
   ServiceLifetime.Transient);

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
               options.UseNpgsql(settings.CoreDatabaseConfiguration.ConnectionString),
               ServiceLifetime.Transient);

builder.Services.AddTransient<IUserRepository, Vanilla_App.Services.Users.Repository.UserRepository>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IProjectRepository, ProjectRepository>();
builder.Services.AddTransient<IProjectService, ProjectService>();

builder.Services.AddMassTransit(x =>
{

    x.UsingRabbitMq((context, cfg) =>
    {
        //x.AddRequestClient<MessageConsumer>(new Uri("exchange:tg-user"));

        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});


builder.Services.AddControllers();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

new UserAPI(app, builder.Services.BuildServiceProvider());

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // Map attribute-routed API controllers
});

app.Run();


class UserAPI
{
    WebApplication _app;
    UserService _userService;
    IProjectService _projectService;
    SettingsModel _settings;
    IRequestClient<TgUserRequest> tgRequestClient;

    public UserAPI(WebApplication app, ServiceProvider serviceProvider)
    {
        _app = app;
        _userService = serviceProvider.GetService<UserService>();
        _projectService = serviceProvider.GetService<IProjectService>();

        ConfigurationMeneger confManager = new ConfigurationMeneger();
        _settings = confManager.Settings;

        RouteRegistration();
    }


    void RouteRegistration()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\", "storage");
        var filePath = new PhysicalFileProvider(path);

        _app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = filePath,
            RequestPath = "/storage"
        });
        _app.MapGet("/", () => "Hello World!");
        _app.MapGet("/health", () => "online");
    }
}

static class TestMinimalAPI
{
    public async static void RunMinimalApi(WebApplication app, ServiceProvider serviceProvider)
    {
        //var userService = serviceProvider.GetService<IUserService>();
        var oauthUserRepository = serviceProvider.GetService<Vanilla.OAuth.Services.UserRepository>();
        var userService = serviceProvider.GetService<UserService>();

        ConfigurationMeneger confManager = new ConfigurationMeneger();
        var _settings = confManager.Settings;

        async Task<string> PrintProfilesList()
        {
            //var users = userService.GetUsers().Result.Where(x => x.IsHasProfile == true);
            var users = await userService.GetUsers();

            var message = "";

            foreach (var user in users)
            {
                var nickanme = oauthUserRepository.GetUserAsync(user.Id).Result.Nickname;

                var profileUrl = String.Format("https://{0}/users/{1}", _settings.Domain, user.Id.ToString());
                message += String.Format("**[{0}]({1})** \n --- \n\n", nickanme, profileUrl);
            }

            return Markdown.ToHtml(message);
        }

        async Task<string> PrintProfileInformation(Guid userId)
        {
            //var user = userService.GetUser(userId).Result;

            var oauthUserRepository = serviceProvider.GetService<Vanilla.OAuth.Services.UserRepository>();
            var userService = serviceProvider.GetService<UserService>();
            var projectsService = serviceProvider.GetService<IProjectService>();

            var nickanme = oauthUserRepository.GetUserAsync(userId).Result.Nickname;
            var user = await userService.GetUser(userId);

            //var userProfile = "![" + nickanme + "](https://" + _settings.Domain + "/storage/" + user.Images.FirstOrDefault().TgMediaId + ".jpg) \r\n\n**" + nickanme + "**\r\n\r\n" + user.About + "\r\n\r\n\r\n";
            var userProfile = "**" + nickanme + "**\r\n\r\n" + user.About + "\r\n\r\n\r\n";

            //var links = String.Format("[{0}](http://t.me/{1})\r\n", user.Username, user.Username);
            var links = "";
            if (user.Links is not null)
            {
                foreach (var link in user.Links)
                {
                    links += String.Format("[{0}]({1})\r\n", link, link);
                }
            }
            userProfile += links;

            var userProjects = "\n\n---\r\n";

            var projects = projectsService.ProjectGetAllAsync().Result.Where(x => x.OwnerId == user.Id);
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


/*        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();*/


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

                var result = await PrintProfilesList();
                await context.Response.WriteAsync(result, Encoding.UTF8);

            });

        app.MapGet("/users/{userId}",
            async context =>
            {
                context.Response.ContentType = "text/html; charset=UTF8";

                var someValueFromGet = context.Response;

                if (context.Request.RouteValues.ContainsKey("userId"))
                {
                    var result = await PrintProfileInformation(Guid.Parse(context.Request.RouteValues["userId"].ToString()));
                    await context.Response.WriteAsync(result, Encoding.UTF8);
                }

            });

    }
}
