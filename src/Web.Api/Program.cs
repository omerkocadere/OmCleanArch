using CleanArch.Application;
using CleanArch.Infrastructure;
using CleanArch.Infrastructure.BackgroundJobs;
using CleanArch.Infrastructure.Data;
using CleanArch.Infrastructure.OpenTelemetry;
using CleanArch.Web.Api;
using CleanArch.Web.Api.Extensions;
using Hangfire;
using Serilog;

Console.WriteLine("Application starting...");
EnvironmentInspector.LoadAndPrintAll();

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureOpenTelemetryInHouse();

// builder.AddServiceDefaults();
// builder.AddSeqEndpoint("om-seq");

builder.Host.UseSerilog(
    (context, loggerConfig) =>
    {
        loggerConfig.ReadFrom.Configuration(context.Configuration);
    }
);

builder
    .Services.AddApplicationServices()
    .AddInfrastructureServices(builder.Environment, builder.Configuration)
    .AddWebServices(builder.Configuration)
    .AddRateLimiting();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "LocalCorsPolicy",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:4200",
                    "https://localhost:4200",
                    "http://localhost:4201",
                    "https://localhost:4201"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
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

// app.UseAuthentication();
app.UseAuthorization();

app.UseRequestContextLogging();

//app.UseSerilogRequestLogging();

app.Map("/", () => Results.Redirect("/api"));
app.MapEndpoints();
app.MapControllers();

app.InitializeBackgroundJobsConditionally();

await app.RunAsync();
