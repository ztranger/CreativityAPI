var builder = DistributedApplication.CreateBuilder(args);

//var cache = builder.AddRedis("cache");

var apiService = builder.AddProject<Projects.API_ApiService>("apiservice");

builder.AddProject<Projects.API_Web>("webfrontend")
    .WithExternalHttpEndpoints()
 //   .WithReference(cache)
//    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
