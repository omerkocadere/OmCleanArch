using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Domain.Auctions;
using CleanArch.Domain.Items;
using CleanArch.Domain.Members;
using CleanArch.Domain.Permissions;
using CleanArch.Domain.Photos;
using CleanArch.Domain.Roles;
using CleanArch.Domain.TodoItems;
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
        var permissionsJsonPath = Path.Combine(seedDir, "permissions.json");
        var rolesJsonPath = Path.Combine(seedDir, "roles.json");
        logger.LogInformation("Users JSON path: {UsersJsonPath}", usersJsonPath);

        await SeedPermissionsAsync(context, logger, permissionsJsonPath);
        await SeedRolesAsync(context, logger, rolesJsonPath);

        var createdUserIds = await SeedUsersAsync(context, logger, passwordHasher, usersJsonPath);

        await SeedTodoListsAsync(context, logger, listsJsonPath, createdUserIds);

        await SeedAuctionsAsync(context, logger, auctionsJsonPath);
    }

    private static async Task SeedPermissionsAsync(
        ApplicationDbContext context,
        ILogger logger,
        string permissionsJsonPath
    )
    {
        if (await context.Permissions.AnyAsync())
            return;

        if (!File.Exists(permissionsJsonPath))
        {
            logger.LogWarning("Permissions seed file not found: {Path}", permissionsJsonPath);
            return;
        }

        logger.LogInformation("Seeding permissions from {Path}", permissionsJsonPath);
        var permissionsJson = await File.ReadAllTextAsync(permissionsJsonPath);
        var permissionsData = JsonSerializer.Deserialize<List<PermissionSeedDto>>(permissionsJson, _jsonOptions);

        if (permissionsData is null || permissionsData.Count == 0)
        {
            logger.LogWarning("No data found in permissions.json");
            return;
        }

        foreach (var permissionDto in permissionsData)
        {
            var permission = new Permission
            {
                Id = permissionDto.Id,
                Name = permissionDto.Name,
                DisplayName = permissionDto.DisplayName,
                Description = permissionDto.Description,
                Category = permissionDto.Category,
                IsActive = permissionDto.IsActive,
            };
            context.Permissions.Add(permission);
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {PermissionCount} permissions.", permissionsData.Count);
    }

    private static async Task SeedRolesAsync(ApplicationDbContext context, ILogger logger, string rolesJsonPath)
    {
        if (await context.Roles.AnyAsync())
            return;

        if (!File.Exists(rolesJsonPath))
        {
            logger.LogWarning("Roles seed file not found: {Path}", rolesJsonPath);
            return;
        }

        logger.LogInformation("Seeding roles with permissions from {Path}", rolesJsonPath);
        var rolesJson = await File.ReadAllTextAsync(rolesJsonPath);
        var rolesData = JsonSerializer.Deserialize<List<RoleSeedDto>>(rolesJson, _jsonOptions);

        if (rolesData is null || rolesData.Count == 0)
        {
            logger.LogWarning("No data found in roles.json");
            return;
        }

        // Load all permissions for mapping
        var permissions = await context.Permissions.ToListAsync();

        foreach (var roleDto in rolesData)
        {
            var role = new Role
            {
                Id = roleDto.Id,
                Name = roleDto.Name,
                NormalizedName = roleDto.NormalizedName,
                Description = roleDto.Description,
                IsDefault = roleDto.IsDefault,
                IsActive = roleDto.IsActive,
            };

            // Add permissions to role
            foreach (var permissionId in roleDto.Permissions)
            {
                var permission = permissions.FirstOrDefault(p => p.Id == permissionId);
                if (permission != null)
                {
                    role.AddPermission(permission);
                }
            }

            context.Roles.Add(role);
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {RoleCount} roles with permissions.", rolesData.Count);
    }

    private static async Task<List<Guid>> SeedUsersAsync(
        ApplicationDbContext context,
        ILogger logger,
        IPasswordHasher passwordHasher,
        string usersJsonPath
    )
    {
        List<Guid> createdUserIds = [];
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
                Email = userDto.Email.ToLower(),
                UserName = userDto.Email.ToLower(),
                DisplayName = userDto.DisplayName,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                ImageUrl = userDto.ImageUrl,
                Member = new Member
                {
                    Id = userDto.Id,
                    DateOfBirth = DateOnly.FromDateTime(DateTime.SpecifyKind(userDto.DateOfBirth, DateTimeKind.Utc)),
                    ImageUrl = userDto.ImageUrl,
                    DisplayName = userDto.DisplayName,
                    LastActive = DateTime.SpecifyKind(userDto.LastActive, DateTimeKind.Utc),
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

        // Assign roles to users
        logger.LogInformation("Assigning roles to users...");

        // Load roles from database
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Administrator");
        var memberRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Member");

        if (adminRole == null || memberRole == null)
        {
            logger.LogWarning("Could not find Administrator or Member roles for user assignment");
        }
        else
        {
            // Load users with their roles navigation property
            var users = await context
                .Users.Include(u => u.Roles)
                .Where(u => userData.Select(dto => dto.Id).Contains(u.Id))
                .ToListAsync();

            foreach (var user in users)
            {
                // Assign Administrator role to first user, Member role to others
                var roleToAssign = user.Id == userData[0].Id ? adminRole : memberRole;
                user.AssignRole(roleToAssign);
            }

            await context.SaveChangesAsync();
            logger.LogInformation(
                "Assigned Administrator role to first user, Member role to {MemberCount} other users.",
                users.Count - 1
            );
        }

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
        if (await context.TodoLists.AnyAsync() || createdUserIds.Count == 0)
            return;

        if (!File.Exists(listsJsonPath))
        {
            logger.LogWarning("Seed file not found: {Path}", listsJsonPath);
            return;
        }

        logger.LogInformation("Seeding todolists from {Path}", listsJsonPath);
        var listsJson = await File.ReadAllTextAsync(listsJsonPath);
        var listsData = JsonSerializer.Deserialize<List<TodoListSeedDto>>(listsJson, _jsonOptions);
        if (listsData is null || listsData.Count == 0)
        {
            logger.LogWarning("No data found in todolists.json");
            return;
        }

        var users = await context.Users.ToDictionaryAsync(u => u.Id);

        for (int i = 0; i < listsData.Count; i++)
        {
            var listDto = listsData[i];
            // Assign users in round-robin fashion
            var userId = createdUserIds[i % createdUserIds.Count];

            var todoList = new TodoList
            {
                Title = listDto.Title,
                Colour = Colour.From(listDto.Colour),
                UserId = userId,
                User = users[userId],
            };

            // Add items to the list
            if (listDto.Items?.Any() is true)
            {
                foreach (var itemDto in listDto.Items)
                {
                    var todoItem = new TodoItem
                    {
                        Title = itemDto.Title,
                        Note = itemDto.Note,
                        Priority = (PriorityLevel)itemDto.Priority,
                        Reminder = itemDto.Reminder?.ToUniversalTime(),
                        Description = itemDto.Description,
                        DueDate = itemDto.DueDate?.ToUniversalTime(),
                        Labels = itemDto.Labels ?? [],
                        Done = itemDto.Done,
                        CompletedAt = itemDto.CompletedAt?.ToUniversalTime(),
                        UserId = userId,
                        User = users[userId],
                        List = todoList,
                    };

                    todoList.Items.Add(todoItem);
                }
            }

            context.TodoLists.Add(todoList);
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {ListCount} lists with proper User IDs.", listsData.Count);
    }

    private static async Task SeedAuctionsAsync(ApplicationDbContext context, ILogger logger, string auctionsJsonPath)
    {
        if (await context.Auctions.AnyAsync() || !File.Exists(auctionsJsonPath))
            return;

        logger.LogInformation("Seeding auctions from {Path}", auctionsJsonPath);
        var auctionsJson = await File.ReadAllTextAsync(auctionsJsonPath);
        var auctionsData = JsonSerializer.Deserialize<List<AuctionSeedDto>>(auctionsJson, _jsonOptions);
        if (auctionsData is null || auctionsData.Count == 0)
        {
            logger.LogWarning("No data found in auctions.json");
            return;
        }

        foreach (var auctionData in auctionsData)
        {
            var auction = new Auction
            {
                ReservePrice = auctionData.ReservePrice,
                Seller = auctionData.Seller,
                Winner = auctionData.Winner,
                SoldAmount = auctionData.SoldAmount,
                CurrentHighBid = auctionData.CurrentHighBid,
                AuctionEnd = DateTime.SpecifyKind(auctionData.AuctionEnd, DateTimeKind.Utc),
                Status = Enum.Parse<Status>(auctionData.Status),
                Item = new Item
                {
                    Make = auctionData.Item.Make,
                    Model = auctionData.Item.Model,
                    Year = auctionData.Item.Year,
                    Color = auctionData.Item.Color,
                    Mileage = auctionData.Item.Mileage,
                    ImageUrl = auctionData.Item.ImageUrl,
                    Auction = null!, // Will be set below
                },
            };

            // Set the circular reference
            auction.Item.Auction = auction;
            context.Auctions.Add(auction);
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {AuctionCount} auctions.", auctionsData.Count);
    }
}

// DTO for seeding users from JSON
public class UserSeedDto
{
    public required Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Gender { get; set; }
    public required DateTime DateOfBirth { get; set; }
    public required string DisplayName { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required DateTime Created { get; set; }
    public required DateTime LastActive { get; set; }
    public required string Description { get; set; }
    public required string City { get; set; }
    public required string Country { get; set; }
    public required string ImageUrl { get; set; }
}

// DTO for seeding TodoLists from JSON
public class TodoListSeedDto
{
    public required string Title { get; set; }
    public required string Colour { get; set; }
    public List<TodoItemSeedDto>? Items { get; set; }
}

// DTO for seeding TodoItems from JSON
public class TodoItemSeedDto
{
    public required string Title { get; set; }
    public string? Note { get; set; }
    public int Priority { get; set; }
    public DateTime? Reminder { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public List<string>? Labels { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool Done { get; set; }
}

// DTO for seeding Auctions from JSON
public class AuctionSeedDto
{
    public int ReservePrice { get; set; }
    public required string Seller { get; set; }
    public string? Winner { get; set; }
    public int? SoldAmount { get; set; }
    public int? CurrentHighBid { get; set; }
    public required DateTime AuctionEnd { get; set; }
    public required string Status { get; set; }
    public required ItemSeedDto Item { get; set; }
}

// DTO for seeding Items from JSON
public class ItemSeedDto
{
    public required string Make { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public required string Color { get; set; }
    public int Mileage { get; set; }
    public required string ImageUrl { get; set; }
}

// DTO for seeding Permissions from JSON
public class PermissionSeedDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public required string Description { get; set; }
    public required string Category { get; set; }
    public bool IsActive { get; set; } = true;
}

// DTO for seeding Roles from JSON
public class RoleSeedDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string NormalizedName { get; set; }
    public required string Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Guid> Permissions { get; set; } = [];
}
