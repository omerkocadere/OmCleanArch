using CleanArch.Application;
using CleanArch.Infrastructure;
using CleanArch.Infrastructure.BackgroundJobs;
using CleanArch.Infrastructure.Data;
using CleanArch.Web;
using CleanArch.Web.Extensions;
using Hangfire;
using OpenTelemetry.Logs;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.UseSerilog(
    (context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration)
);

builder
    .Services.AddApplicationServices()
    .AddInfrastructureServices(builder.Environment, builder.Configuration)
    .AddWebServices();

// builder.Logging.AddOpenTelemetry(logging =>
// {
//     logging.AddOtlpExporter(options =>
//     {
//         options.Endpoint = new Uri("http://localhost:18889");
//     });
// });

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
    app.UseSwaggerWithUi();

    app.UseHangfireDashboard("/hangfire");
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapHealthChecks("/health");

app.UseHttpsRedirection();

app.UseRequestContextLogging();

// app.UseSerilogRequestLogging();

app.UseStaticFiles();

app.Map("/", () => Results.Redirect("/api"));
app.MapEndpoints();
app.MapControllers();

app.InitializeBackgroundJobs();

app.Run();
