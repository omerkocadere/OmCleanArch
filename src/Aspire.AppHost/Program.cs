var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresDatabaseResource> database = builder
    .AddPostgres("database")
    .WithBindMount("../../.containers/db", "/var/lib/postgresql/data")
    .AddDatabase("OmCleanArch");

builder
    .AddProject<Projects.Web_Api>("web-api")
    .WithEnvironment("DatabaseSettings__PostgresConnectionString", database)
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
