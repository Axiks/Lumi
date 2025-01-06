using Aspire.Hosting;
using Microsoft.Extensions.Hosting;
using System;

var builder = DistributedApplication.CreateBuilder(args);

var usernameDB = builder.AddParameter("usernameDB", secret: true);
var passwordDB = builder.AddParameter("passwordDB", secret: true);
var portDB = builder.AddParameter("portDB", secret: false);
var postgres = builder.AddPostgres("lumi-db", usernameDB, passwordDB, Convert.ToUInt16(portDB.Resource.Value))
    .WithDataVolume()
    .WithPgAdmin();

var coredb = postgres.AddDatabase("coredb", "lumi_core_db");
var oauthdb = postgres.AddDatabase("oauthdb", "lumi_oauth_db");
var tgbotdb = postgres.AddDatabase("tgbotdb", "lumi_tg_bot_db");


var usernameRQ = builder.AddParameter("usernameRQ", secret: true);
var passwordRQ = builder.AddParameter("passwordRQ", secret: true);
var portRQ = builder.AddParameter("portRQ", secret: false);
var messaging = builder.AddRabbitMQ("lumi-mq", usernameRQ, passwordRQ, Convert.ToUInt16(portRQ.Resource.Value))
    .WithManagementPlugin();

var domain = builder.AddParameter("domain", secret: true);
var cdnDomain = builder.AddParameter("cdnDomain", secret: true);
//var apiDomain = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") is "Development" ? "https://localhost:7375" : domain.Resource.Value;

var apiService = builder.AddProject<Projects.Vanilla_Aspire_ApiService>("apiservice")
    .WithEnvironment("DOTNET_ENVIRONMENT", Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"))
    //.WithEnvironment("domain", domain)
    .WithEnvironment("cdnDomain", cdnDomain)
    .WithReference(coredb)
    .WithReference(oauthdb)
    .WithReference(messaging)
    .WaitFor(postgres);

/*    .WithHttpEndpoint(port: 5000)
    .WithHttpsEndpoint(port: 5001);*/
var webFrontend = builder.AddProject<Projects.Vanilla_Aspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    //.WithHttpsEndpoint(port: 8080)
    .WithReference(apiService)
    .WaitFor(apiService);

var botAccessToken = builder.AddParameter("botAccessToken", secret: true);
var botAdminTgId = builder.AddParameter("botAdminTgId", secret: true);
var botGenerealSupergroupTgId = builder.AddParameter("botSupergroupTgId", secret: true);


var provisionBonusApiUrl = builder.AddParameter("provisionBonusApiUrl", secret: true);
var provisionBonusApiAccessToken = builder.AddParameter("provisionBonusApiAccessToken", secret: true);

var tokenPrivateKey = builder.AddParameter("tokenPrivateKey", secret: true);
var tokenLifetimeSec = builder.AddParameter("tokenLifetimeSec", secret: true);
var tokenIssuer = builder.AddParameter("tokenIssuer", secret: true);
var tokenAudience = builder.AddParameter("tokenAudience", secret: true);

var telegramBot = builder.AddProject<Projects.Vanilla_TelegramBot>("telegrambot")
    .WithEnvironment("DOTNET_ENVIRONMENT", Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"))
    .WithReference(coredb)
    .WithReference(oauthdb)
    .WithReference(tgbotdb)
    .WithReference(messaging)

    .WithEnvironment("domain", domain)
    .WithEnvironment("cdnDomain", cdnDomain)
    .WithEnvironment("botAccessToken", botAccessToken)
    .WithEnvironment("provisionBonusApiUrl", provisionBonusApiUrl)
    .WithEnvironment("provisionBonusApiAccessToken", provisionBonusApiAccessToken)

    .WithEnvironment("tokenPrivateKey", tokenPrivateKey)
    .WithEnvironment("tokenLifetimeSec", tokenLifetimeSec)
    .WithEnvironment("tokenIssuer", tokenIssuer)
    .WithEnvironment("tokenAudience", tokenAudience)


    .WaitFor(messaging)
    .WaitFor(postgres);

/*var cloudflareToken = builder.AddParameter("cloudflareToken", secret: true);
builder
    .AddContainer("cloudflare-lumi-tunnel", "cloudflare/cloudflared", "latest")
    .WithArgs("tunnel", "run")
    //.WithHttpsEndpoint(port: 8080, targetPort: 8080, isProxied: true)
    .WaitFor(webFrontend)
    //.WithReference(webFrontend)
    .WithEnvironment("TUNNEL_TOKEN", cloudflareToken);*/


builder.Build().Run();
