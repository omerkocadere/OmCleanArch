using System.Security.Claims;
using Duende.IdentityModel;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityService;

public static class SeedData
{
    public static void EnsureSeedData(WebApplication app)
    {
        using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (userMgr.Users.Any())
            return;

        var omer = userMgr.FindByNameAsync("omer").Result;
        if (omer == null)
        {
            omer = new ApplicationUser
            {
                UserName = "omer",
                Email = "omer@example.com",
                EmailConfirmed = true,
            };
            var result = userMgr.CreateAsync(omer, "Asd123!").Result;
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(omer, [new Claim(JwtClaimTypes.Name, "Omer Kocadere")]).Result;
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(result.Errors.First().Description);
            }
            Log.Debug("omer created");
        }
        else
        {
            Log.Debug("omer already exists");
        }

        var bob = userMgr.FindByNameAsync("bob").Result;
        if (bob == null)
        {
            bob = new ApplicationUser
            {
                UserName = "bob",
                Email = "BobSmith@example.com",
                EmailConfirmed = true,
            };
            var result = userMgr.CreateAsync(bob, "Asd123!").Result;
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(bob, [new Claim(JwtClaimTypes.Name, "Bob Smith")]).Result;
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(result.Errors.First().Description);
            }
            Log.Debug("bob created");
        }
        else
        {
            Log.Debug("bob already exists");
        }
    }
}
