// Program.cs (AppHost projesi)
var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL şifresini parameter olarak tanımlayın
var postgresPassword = builder.AddParameter("postgres-password", "postgrespw", secret: true);
var postgresUserName = builder.AddParameter("postgres-username", "postgres", secret: true);

// PostgreSQL container'ını oluşturun ve veritabanını ekleyin
IResourceBuilder<PostgresDatabaseResource> database = builder
    .AddPostgres("om-postgres", userName: postgresUserName, password: postgresPassword, port: 5432)
    .WithBindMount("../../.containers/db", "/var/lib/postgresql/data")
    .AddDatabase("omcleanarch");

// API projenize referans verin
builder.AddProject<Projects.Web_Api>("web-api").WithReference(database).WaitFor(database);

// Dummy API projesini ekleyin
builder.AddProject<Projects.Dummy_Api>("dummy-api");

builder.Build().Run();
