// Program.cs (AppHost projesi)
var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("env");

var postgresPassword = builder.AddParameter("postgres-password", "postgrespw", secret: true);
var postgresUserName = builder.AddParameter("postgres-username", "postgres", secret: true);

IResourceBuilder<PostgresDatabaseResource> database = builder
    .AddPostgres("om-postgres", userName: postgresUserName, password: postgresPassword, port: 5432)
    .WithBindMount("../../.containers/db", "/var/lib/postgresql/data")
    .AddDatabase("omcleanarch");

builder.AddProject<Projects.Web_Api>("web-api").WithReference(database).WaitFor(database);
builder.AddProject<Projects.Dummy_Api>("dummy-api");

builder.Build().Run();
