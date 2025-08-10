using System.Security.Claims;
using System.Threading.RateLimiting;
using CleanArch.Application;
using CleanArch.Infrastructure;
using CleanArch.Infrastructure.BackgroundJobs;
using CleanArch.Infrastructure.Data;
using CleanArch.Infrastructure.OpenTelemetry;
using CleanArch.Web.Api;
using CleanArch.Web.Api.Extensions;
using CleanArch.Web.Api.Playground.Services;
using Hangfire;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
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
    .AddPlayServices(builder.Configuration);

// Rate Limiting configuration
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, token) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = $"{retryAfter.TotalSeconds}";

            var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            var problemDetails = problemDetailsFactory.CreateProblemDetails(
                context.HttpContext,
                StatusCodes.Status429TooManyRequests,
                "Too Many Requests",
                detail: $"Too many requests. Please try again after {retryAfter.TotalSeconds} seconds."
            );

            await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: token);
        }
    };

    options.AddFixedWindowLimiter(
        "fixed",
        opt =>
        {
            opt.PermitLimit = 5;
            opt.Window = TimeSpan.FromMinutes(1);
        }
    );

    options.AddPolicy(
        "per-user",
        httpContext =>
        {
            string? userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                return RateLimitPartition.GetTokenBucketLimiter(
                    userId,
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 7,
                        TokensPerPeriod = 2,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                    }
                );
            }

            return RateLimitPartition.GetFixedWindowLimiter(
                "anonymous",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 3, Window = TimeSpan.FromMinutes(1) }
            );
        }
    );
});

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

var app = builder.Build();
app.UseRateLimiter();

app.MapDefaultEndpoints();

app.UseExceptionHandler();

await app.InitialiseDatabaseAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseHangfireDashboardConditionally();
    app.UseSwaggerWithUi();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

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
