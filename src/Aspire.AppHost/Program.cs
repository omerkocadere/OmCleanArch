// Program.cs (AppHost projesi)
var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL şifresini parameter olarak tanımlayın
var postgresPassword = builder.AddParameter("postgres-password", "postgrespw", secret: true);
var postgresUserName = builder.AddParameter("postgres-username", "postgres", secret: true);

// PostgreSQL container'ını oluşturun ve veritabanını ekleyin
IResourceBuilder<PostgresDatabaseResource> database = builder
    .AddPostgres("database", userName: postgresUserName, password: postgresPassword, port: 5432)
    .WithBindMount("../../.containers/db", "/var/lib/postgresql/data")
    .AddDatabase("OmCleanArch");

// API projenize referans verin
builder.AddProject<Projects.Web_Api>("web-api").WithReference(database).WaitFor(database);

builder.Build().Run();
