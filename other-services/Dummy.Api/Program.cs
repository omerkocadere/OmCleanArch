using System.Collections;
using DotNetEnv;
using Dummy.Api;
using Dummy.Api.Data;

LoadAndPrintAll();

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

builder.AddServiceDefaults();

// builder.AddSeqEndpoint("om-seq");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument();
builder.Services.AddDatabase(builder.Environment, builder.Configuration);

var app = builder.Build();

// app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();

app.MapDummyEndpoints();

app.Run();

static void LoadAndPrintAll()
{
    Env.Load();
    PrintAll();
}

static void PrintAll()
{
    Console.WriteLine("---- ENVIRONMENT VARIABLES ----");
    foreach (
        DictionaryEntry env in Environment
            .GetEnvironmentVariables()
            .Cast<DictionaryEntry>()
            .OrderBy(e => e.Key.ToString(), StringComparer.OrdinalIgnoreCase)
    )
    {
        Console.WriteLine($"{env.Key} = {env.Value}");
    }
    Console.WriteLine("--------------------------------");
}
