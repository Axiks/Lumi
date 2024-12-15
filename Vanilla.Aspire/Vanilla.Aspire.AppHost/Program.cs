var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Vanilla_Aspire_ApiService>("apiservice")
    .WithEnvironment("DOTNET_ENVIRONMENT", Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));
/*    .WithHttpEndpoint(port: 5000)
    .WithHttpsEndpoint(port: 5001);*/
builder.AddProject<Projects.Vanilla_Aspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.AddProject<Projects.Vanilla_TelegramBot>("telegrambot")
    .WithEnvironment("DOTNET_ENVIRONMENT", Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));

builder.Build().Run();
