var builder = DistributedApplication.CreateBuilder(args);

//var cache = builder.AddRedis("cache");

var usersRepositoryProvider = builder.Configuration["UsersRepository:Provider"] ?? "InMemory";
var connectionString = builder.Configuration["ConnectionStrings:Main"]
    ?? "Host=localhost;Port=5432;Database=creativityapi;Username=postgres;Password=postgres";

var apiService = builder.AddProject<Projects.API_ApiService>("apiservice")
    .WithEnvironment("UsersRepository__Provider", usersRepositoryProvider)
    .WithEnvironment("ConnectionStrings__Main", connectionString);

builder.AddProject<Projects.API_Web>("webfrontend")
    .WithExternalHttpEndpoints()
 //   .WithReference(cache)
//    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
