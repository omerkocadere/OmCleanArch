using System.Net;
using Contracts;
using Mapster;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Endpoints;
using SearchService.Models;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetRetryPolicy());
builder.Services.AddHttpClient<AuctionSvcHttpClient>();

builder.Services.ConfigureHttpClientDefaults(http =>
{
    // Turn on resilience by default
    http.AddStandardResilienceHandler();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument();

// Configure Mapster
TypeAdapterConfig.GlobalSettings.Default.NameMatchingStrategy(NameMatchingStrategy.IgnoreCase);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    x.UsingRabbitMq(
        (context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
        }
    );
});

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

app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await app.InitDb();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to initialize database. Application will continue running.");
    }
});

app.Run();

// NOTE: Commented out in favor of AddStandardResilienceHandler() which provides:
// - Better resilience with circuit breaker, rate limiter, and timeouts
// - Finite retry attempts (3) instead of infinite retries
// - Exponential backoff with jitter for better performance
// - Built on Polly v8 with zero-allocation optimizations
// - Handles more HTTP error conditions (500+, 408, 429) out of the box
// - Prevents potential infinite retry loops that could overwhelm services

/*
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));
}
*/
