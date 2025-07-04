using CleanArch.Domain.Users;
using CleanArch.Infrastructure.Data;
using CleanArch.Web.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Web.Endpoints;

public class Test : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this).MapPost(CreateUser);
    }

    public async Task<IResult> CreateUser(ApplicationDbContext context)
    {
        var user = new User
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hashed_password_here",
        };

        user.AddDomainEvent(new UserRegisteredDomainEvent(user));

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return Results.Ok(new { Message = "Domain event triggered", UserId = user.Id });
    }
}
