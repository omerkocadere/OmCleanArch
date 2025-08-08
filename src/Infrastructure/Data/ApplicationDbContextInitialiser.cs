using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Users.Create;
using CleanArch.Domain.Auctions;
using CleanArch.Domain.Products;
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
            logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
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
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task TrySeedAsync(ApplicationDbContext context, ILogger logger, IPasswordHasher passwordHasher)
    {
        // Get the directory where the currently executing assembly is located
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory =
            Path.GetDirectoryName(assemblyLocation)
            ?? throw new InvalidOperationException("Could not determine assembly directory");
        var seedDir = Path.Combine(assemblyDirectory, "Data", "Seed");

        logger.LogInformation("Assembly location: {AssemblyLocation}", assemblyLocation);
        logger.LogInformation("Seed directory resolved to: {SeedDir}", seedDir);

        var usersJsonPath = Path.Combine(seedDir, "users.json");
        var listsJsonPath = Path.Combine(seedDir, "todolists.json");
        var productsJsonPath = Path.Combine(seedDir, "products.json");
        var auctionsJsonPath = Path.Combine(seedDir, "auctions.json");
        logger.LogInformation("Users JSON path: {UsersJsonPath}", usersJsonPath);
        logger.LogInformation("TodoLists JSON path: {ListsJsonPath}", listsJsonPath);

        // Seed Users first and collect their IDs
        List<Guid> createdUserIds = new();
        if (!context.Users.Any())
        {
            if (!File.Exists(usersJsonPath))
            {
                logger.LogWarning("Seed file not found: {Path}", usersJsonPath);
            }
            else
            {
                logger.LogInformation("Seeding users from {Path}", usersJsonPath);
                var usersJson = await File.ReadAllTextAsync(usersJsonPath);
                var usersData = JsonSerializer.Deserialize<List<User>>(usersJson, _jsonOptions);
                if (usersData is null || usersData.Count == 0)
                {
                    logger.LogWarning("No data found in users.json");
                    return;
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
                        // Get the created user's ID from database
                        var createdUser = await context.Users.FirstOrDefaultAsync(u => u.Email == command.Email);
                        if (createdUser != null)
                            createdUserIds.Add(createdUser.Id);
                    }
                }
                logger.LogInformation("Seeded {UserCount} users via CreateUserCommandHandler.", successCount);
            }
        }
        else
        {
            // Users already exist, get their IDs
            createdUserIds = await context.Users.Select(u => u.Id).ToListAsync();
        }

        if (!context.TodoLists.Any() && createdUserIds.Any())
        {
            if (!File.Exists(listsJsonPath))
            {
                logger.LogWarning("Seed file not found: {Path}", listsJsonPath);
            }
            else
            {
                logger.LogInformation("Seeding todolists from {Path}", listsJsonPath);
                var listsJson = await File.ReadAllTextAsync(listsJsonPath);
                var listsData = JsonSerializer.Deserialize<List<TodoList>>(listsJson, _jsonOptions);
                if (listsData is null || listsData.Count == 0)
                {
                    logger.LogWarning("No data found in todolists.json");
                    return;
                }

                // Assign real User IDs to TodoLists cyclically
                for (int i = 0; i < listsData.Count; i++)
                {
                    listsData[i].UserId = createdUserIds[i % createdUserIds.Count];

                    // Also assign User IDs to TodoItems if they exist
                    if (listsData[i].Items?.Any() == true)
                    {
                        foreach (var item in listsData[i].Items)
                        {
                            item.UserId = listsData[i].UserId; // Same user as the list
                        }
                    }
                }

                context.TodoLists.AddRange(listsData);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {ListCount} lists with proper User IDs.", listsData.Count);
            }
        }
        if (!context.Products.Any() && File.Exists(productsJsonPath))
        {
            logger.LogInformation("Seeding products from {Path}", productsJsonPath);
            var productsJson = await File.ReadAllTextAsync(productsJsonPath);
            var products = JsonSerializer.Deserialize<List<Product>>(productsJson, _jsonOptions);
            if (products is null || products.Count == 0)
            {
                logger.LogWarning("No data found in products.json");
                return;
            }
            context.Products.AddRange(products);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {ProductCount} products.", products.Count);
        }

        if (!context.Auctions.Any() && File.Exists(auctionsJsonPath))
        {
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
}
