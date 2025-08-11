using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Users.Create;
using CleanArch.Domain.Auctions;
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

        await ApplicationDbContextInitialiser.InitialiseAsync(context, logger);
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

    public static async Task InitialiseAsync(ApplicationDbContext context, ILogger logger)
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
        var usersData = JsonSerializer.Deserialize<List<User>>(usersJson, _jsonOptions);
        if (usersData is null || usersData.Count == 0)
        {
            logger.LogWarning("No data found in users.json");
            return createdUserIds;
        }

        var handler = new CreateUserCommandHandler(context, passwordHasher);
        int successCount = 0;
        foreach (var user in usersData)
        {
            var command = new CreateUserCommand
            {
                Email = user.Email,
                DisplayName = user.DisplayName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Password = "111111",
            };
            var result = await handler.Handle(command, default);
            if (result.IsSuccess)
            {
                successCount++;
                var createdUser = await context.Users.FirstOrDefaultAsync(u => u.Email == command.Email);
                if (createdUser != null)
                    createdUserIds.Add(createdUser.Id);
            }
        }
        logger.LogInformation("Seeded {UserCount} users via CreateUserCommandHandler.", successCount);
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
