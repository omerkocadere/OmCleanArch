using DotNetEnv;
using Dummy.Api;
using Dummy.Api.Data;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

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
