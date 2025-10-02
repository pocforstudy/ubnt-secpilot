using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// MongoDB for data storage
var mongodb = builder.AddMongoDB("mongodb")
    .WithMongoExpress()
    .WithLifetime(ContainerLifetime.Persistent);

// Redis for caching (optional)
var redis = builder.AddRedis("redis")
    .WithRedisCommander()
    .WithLifetime(ContainerLifetime.Persistent);

// Orleans Silo for distributed processing
var orleansSilo = builder.AddProject("UbntSecPilot.Agents.Orleans", "orleans-silo")
    .WithReference(mongodb)
    .WithReference(redis);

// API service
var api = builder.AddProject("UbntSecPilot.WebApi", "api")
    .WithReference(mongodb)
    .WithReference(redis)
    .WithReference(orleansSilo)
    .WithExternalHttpEndpoints();

// Blazor UI with Aspire dashboard
var blazor = builder.AddProject("UbntSecPilot.BlazorUI", "blazor-ui")
    .WithReference(api)
    .WithExternalHttpEndpoints();

// Enable Aspire dashboard
builder.Build().Run();
