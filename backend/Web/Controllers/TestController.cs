using CleanArch.Domain.Users;
using CleanArch.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace CleanArch.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController(ApplicationDbContext context, ILogger<TestController> logger)
    : ControllerBase
{
    [HttpGet("log-test")]
    public IActionResult LogTest()
    {
        logger.LogInformation("Test log message for Grafana Cloud OTLP");
        logger.LogWarning("This is a warning log for testing");
        logger.LogError("This is an error log for testing");
        return Ok(new { Message = "Logs sent to Grafana via OTLP", Timestamp = DateTime.UtcNow });
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser()
    {
        logger.LogInformation("Creating a new user via Test endpoint");
        // Create a user and trigger domain event
        var user = new User
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hashed_password_here",
        };

        // This will trigger the domain event via the entity
        user.AddDomainEvent(new UserRegisteredDomainEvent(user));

        context.Set<User>().Add(user);
        await context.SaveChangesAsync();

        return Ok(new { Message = "Domain event triggered", UserId = user.Id });
    }
}
