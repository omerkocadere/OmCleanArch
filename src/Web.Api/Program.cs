using CleanArch.Application;
using CleanArch.Infrastructure;
using CleanArch.Infrastructure.BackgroundJobs;
using CleanArch.Infrastructure.Data;
using CleanArch.Infrastructure.OpenTelemetry;
using CleanArch.Web.Api;
using CleanArch.Web.Api.Extensions;
using CleanArch.Web.Api.Playground.Services;
using Hangfire;
using Serilog;

Console.WriteLine("Application starting...");
EnvironmentInspector.LoadAndPrintAll();

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureOpenTelemetryInHouse();

// builder.AddServiceDefaults();
builder.AddSeqEndpoint("om-seq");

// builder.Host.UseSerilog(
//     (context, loggerConfig) =>
//     {
//         loggerConfig.ReadFrom.Configuration(context.Configuration);
//     }
// );


builder
    .Services.AddApplicationServices()
    .AddInfrastructureServices(builder.Environment, builder.Configuration)
    .AddWebServices(builder.Configuration)
    .AddPlayServices(builder.Configuration)
    .AddRateLimiting();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "LocalCorsPolicy",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200", "https://localhost:4200").AllowAnyHeader().AllowAnyMethod();
        }
    );
});

builder.Services.AddStackExchangeRedisCache(redisOptions =>
{
    var connection = builder.Configuration.GetConnectionString("Redis");
    if (string.IsNullOrWhiteSpace(connection))
    {
        throw new InvalidOperationException("Redis connection string is not configured.");
    }

    redisOptions.Configuration = connection;
});

var app = builder.Build();
app.UseRateLimiter();

app.MapDefaultEndpoints();

app.UseExceptionHandler();

await app.InitialiseDatabaseAsync();

app.UseHangfireDashboardConditionally();
app.UseSwaggerWithUi();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("LocalCorsPolicy");

app.UseRequestContextLogging();

//app.UseSerilogRequestLogging();

app.Map("/", () => Results.Redirect("/api"));
app.MapEndpoints();
app.MapControllers();

app.InitializeBackgroundJobsConditionally();

await app.RunAsync();
