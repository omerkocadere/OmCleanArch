using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Domain.Auctions;
using CleanArch.Domain.Members;
using CleanArch.Domain.Photos;
using CleanArch.Domain.TodoLists;
using CleanArch.Domain.Users;
using CleanArch.Domain.ValueObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanArch.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var logger = scope
            .ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(ApplicationDbContextInitialiser));
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await ApplicationDbContextInitialiser.InitialiseAsync(context);
        await ApplicationDbContextInitialiser.SeedAsync(context, logger, passwordHasher);
    }
}

public static class ApplicationDbContextInitialiser
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new ColourJsonConverter(), new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public static async Task InitialiseAsync(ApplicationDbContext context)
    {
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Failed to initialise the database. See inner exception for details.",
                ex
            );
        }
    }

    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger, IPasswordHasher passwordHasher)
    {
        try
        {
            await TrySeedAsync(context, logger, passwordHasher);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to seed the database. See inner exception for details.", ex);
        }
    }

    private static async Task TrySeedAsync(ApplicationDbContext context, ILogger logger, IPasswordHasher passwordHasher)
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory =
            Path.GetDirectoryName(assemblyLocation)
            ?? throw new InvalidOperationException("Could not determine assembly directory");
        var seedDir = Path.Combine(assemblyDirectory, "Data", "Seed");
        logger.LogInformation("Seed directory resolved to: {SeedDir}", seedDir);

        var usersJsonPath = Path.Combine(seedDir, "users.json");
        var listsJsonPath = Path.Combine(seedDir, "todolists.json");
        var auctionsJsonPath = Path.Combine(seedDir, "auctions.json");
        logger.LogInformation("Users JSON path: {UsersJsonPath}", usersJsonPath);

        var createdUserIds = await SeedUsersAsync(context, logger, passwordHasher, usersJsonPath);

        await SeedTodoListsAsync(context, logger, listsJsonPath, createdUserIds);

        await SeedAuctionsAsync(context, logger, auctionsJsonPath);
    }

    private static async Task<List<Guid>> SeedUsersAsync(
        ApplicationDbContext context,
        ILogger logger,
        IPasswordHasher passwordHasher,
        string usersJsonPath
    )
    {
        var culture = System.Globalization.CultureInfo.InvariantCulture;
        List<Guid> createdUserIds = new();
        if (await context.Users.AnyAsync())
        {
            createdUserIds = await context.Users.Select(u => u.Id).ToListAsync();
            return createdUserIds;
        }

        if (!File.Exists(usersJsonPath))
        {
            logger.LogWarning("Seed file not found: {Path}", usersJsonPath);
            return createdUserIds;
        }

        logger.LogInformation("Seeding users from {Path}", usersJsonPath);
        var usersJson = await File.ReadAllTextAsync(usersJsonPath);
        var userData = JsonSerializer.Deserialize<List<UserSeedDto>>(usersJson, _jsonOptions);
        if (userData is null || userData.Count == 0)
        {
            logger.LogWarning("No data found in users.json");
            return createdUserIds;
        }

        foreach (var userDto in userData)
        {
            var user = new User
            {
                Id = userDto.Id,
                Email = userDto.Email,
                DisplayName = userDto.DisplayName,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                PasswordHash = passwordHasher.Hash("111111"),
                Member = new Member
                {
                    Id = userDto.Id,
                    DateOfBirth = DateOnly.Parse(userDto.DateOfBirth, culture),
                    ImageUrl = userDto.ImageUrl,
                    DisplayName = userDto.DisplayName,
                    LastActive = DateTime.Parse(userDto.LastActive, culture),
                    Gender = userDto.Gender,
                    Description = userDto.Description,
                    City = userDto.City,
                    Country = userDto.Country,
                    User = null!, // Will be set by EF Core
                },
            };

            // Create a photo for this member using their ImageUrl
            if (!string.IsNullOrEmpty(userDto.ImageUrl))
            {
                var photo = new Photo
                {
                    Url = userDto.ImageUrl,
                    MemberId = userDto.Id,
                    Member = user.Member,
                };

                context.Photos.Add(photo);
            }

            context.Users.Add(user);
        }

        await context.SaveChangesAsync();

        // Collect the created user IDs
        createdUserIds = [.. userData.Select(u => u.Id)];

        logger.LogInformation("Seeded {UserCount} users with corresponding members and photos.", userData.Count);
        return createdUserIds;
    }

    private static async Task SeedTodoListsAsync(
        ApplicationDbContext context,
        ILogger logger,
        string listsJsonPath,
        List<Guid> createdUserIds
    )
    {
        if (await context.TodoLists.AnyAsync() || !createdUserIds.Any())
            return;

        if (!File.Exists(listsJsonPath))
        {
            logger.LogWarning("Seed file not found: {Path}", listsJsonPath);
            return;
        }

        logger.LogInformation("Seeding todolists from {Path}", listsJsonPath);
        var listsJson = await File.ReadAllTextAsync(listsJsonPath);
        var listsData = JsonSerializer.Deserialize<List<TodoList>>(listsJson, _jsonOptions);
        if (listsData is null || listsData.Count == 0)
        {
            logger.LogWarning("No data found in todolists.json");
            return;
        }

        for (int i = 0; i < listsData.Count; i++)
        {
            listsData[i].UserId = createdUserIds[i % createdUserIds.Count];

            if (listsData[i].Items?.Any() == true)
            {
                foreach (var item in listsData[i].Items)
                {
                    item.UserId = listsData[i].UserId;
                }
            }
        }

        context.TodoLists.AddRange(listsData);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {ListCount} lists with proper User IDs.", listsData.Count);
    }

    private static async Task SeedAuctionsAsync(ApplicationDbContext context, ILogger logger, string auctionsJsonPath)
    {
        if (await context.Auctions.AnyAsync() || !File.Exists(auctionsJsonPath))
            return;

        logger.LogInformation("Seeding auctions from {Path}", auctionsJsonPath);
        var auctionsJson = await File.ReadAllTextAsync(auctionsJsonPath);
        var auctions = JsonSerializer.Deserialize<List<Auction>>(auctionsJson, _jsonOptions);
        if (auctions is null || auctions.Count == 0)
        {
            logger.LogWarning("No data found in auctions.json");
            return;
        }

        context.Auctions.AddRange(auctions);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {AuctionCount} auctions.", auctions.Count);
    }
}

// DTO for seeding users from JSON
public class UserSeedDto
{
    public required Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Gender { get; set; }
    public required string DateOfBirth { get; set; }
    public required string DisplayName { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Created { get; set; }
    public required string LastActive { get; set; }
    public required string Description { get; set; }
    public required string City { get; set; }
    public required string Country { get; set; }
    public required string ImageUrl { get; set; }
}
