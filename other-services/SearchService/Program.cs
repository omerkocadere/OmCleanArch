using SearchService.Data;
using SearchService.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();

// Map endpoints
app.MapWeatherForecastEndpoints();
app.MapSearchEndpoints();
await app.InitDb();

app.Run();
