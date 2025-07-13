var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddSeqEndpoint("om-seq");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/hellofromdummy", () => "Hello, world from Dummy API!");

app.Run();
