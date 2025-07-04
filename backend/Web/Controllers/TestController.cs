using CleanArch.Domain.Users;
using CleanArch.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace CleanArch.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TestController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser()
    {
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

        _context.Set<User>().Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Domain event triggered", UserId = user.Id });
    }
}
