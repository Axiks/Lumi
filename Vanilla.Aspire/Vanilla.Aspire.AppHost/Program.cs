var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Vanilla_Aspire_ApiService>("apiservice");

builder.AddProject<Projects.Vanilla_Aspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

//builder.AddProject< Projects.>;

builder.Build().Run();
