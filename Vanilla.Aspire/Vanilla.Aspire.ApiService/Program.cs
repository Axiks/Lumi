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

/*var settings = new ConfigurationMeneger().Settings;
if (settings == null) throw new Exception("No found setting section");*/

builder.Services.AddTransient<StorageModule>();
builder.Services.AddTransient<Vanilla.OAuth.Services.UserRepository>();

/*builder.Services.AddDbContextFactory<Vanilla.OAuth.ApplicationDbContext>(options =>
   options.UseNpgsql(settings.OAuthDatabaseConfiguration.ConnectionString),
   ServiceLifetime.Transient);

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
               options.UseNpgsql(settings.CoreDatabaseConfiguration.ConnectionString),
               ServiceLifetime.Transient);*/


var connectionStringOAuthDb = builder.Configuration.GetConnectionString("oauthdb");
builder.Services.AddDbContextFactory<Vanilla.OAuth.ApplicationDbContext>(options =>
   options.UseNpgsql(connectionStringOAuthDb),
   ServiceLifetime.Transient);

var connectionStringCoreDb = builder.Configuration.GetConnectionString("coredb");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
               options.UseNpgsql(connectionStringCoreDb),
               ServiceLifetime.Transient);

builder.Services.AddTransient<IUserRepository, Vanilla_App.Services.Users.Repository.UserRepository>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IProjectRepository, ProjectRepository>();
builder.Services.AddTransient<IProjectService, ProjectService>();

builder.Services.AddMassTransit(x =>
{

    x.UsingRabbitMq((context, cfg) =>
    {
        var connectionStringRQ = builder.Configuration.GetConnectionString("lumi-mq");
        cfg.Host(connectionStringRQ);

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

app.MapDefaultEndpoints();

app.Run();


class UserAPI
{
    WebApplication _app;

    public UserAPI(WebApplication app, ServiceProvider serviceProvider)
    {
        _app = app;
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        RouteRegistration(logger);
    }

    void RouteRegistration(ILogger<Program> logger)
    {
        var rootPath = Path.Combine(Directory.GetCurrentDirectory(), @"../../");
        var storagePath = Path.Combine(rootPath, "storage");
        //System.IO.Directory.CreateDirectory(storagePath);
        var filePath = new PhysicalFileProvider(storagePath);

        _app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = filePath,
            RequestPath = "/storage"
        });
        _app.MapGet("/", () => { logger.LogInformation("Hello World too!"); return "Hello World!"; });
        _app.MapGet("/health", () => "online");
    }
}