var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("env");

var postgresPassword = builder.AddParameter("postgres-password", "postgrespw", secret: true);
var postgresUserName = builder.AddParameter("postgres-username", "postgres", secret: true);

var seq = builder.AddSeq("om-seq").WithEnvironment("ACCEPT_EULA", "Y").WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<PostgresDatabaseResource> database = builder
    .AddPostgres("om-postgres", userName: postgresUserName, password: postgresPassword, port: 5432)
    .WithBindMount("../../.containers/db", "/var/lib/postgresql/data")
    .AddDatabase("omcleanarch");

builder
    .AddProject<Projects.Web_Api>("web-api")
    // .PublishAsDockerFile()
    .WithReference(database)
    .WaitFor(database)
    .WithReference(seq)
    .WaitFor(seq);

builder.AddProject<Projects.Dummy_Api>("dummy-api").WithReference(seq).WaitFor(seq);

builder.AddProject<Projects.GatewayService>("gatewayservice");

builder.Build().Run();
