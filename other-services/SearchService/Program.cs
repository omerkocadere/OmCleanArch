using SearchService.Data;
using SearchService.Endpoints;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<AuctionSvcHttpClient>();

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
