using System.Linq.Expressions;
using System.Reflection;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Domain.Auctions;
using CleanArch.Domain.Common;
using CleanArch.Domain.Members;
using CleanArch.Domain.Messages;
using CleanArch.Domain.Permissions;
using CleanArch.Domain.Photos;
using CleanArch.Domain.Roles;
using CleanArch.Domain.TodoItems;
using CleanArch.Domain.TodoLists;
using CleanArch.Domain.Users;
using CleanArch.Infrastructure.BackgroundJobs.Outbox;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options),
        IApplicationDbContext
{
    public DbSet<TodoList> TodoLists => Set<TodoList>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Auction> Auctions => Set<Auction>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<MemberLike> Likes => Set<MemberLike>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<OutboxMessageConsumer> OutboxMessageConsumers => Set<OutboxMessageConsumer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configure global query filters for soft delete
        // This automatically excludes soft-deleted entities from all queries
        ConfigureSoftDeleteQueryFilters(modelBuilder);

        // Add MassTransit outbox tables under auction schema
        modelBuilder.AddInboxStateEntity(cfg => cfg.ToTable("InboxState", "carsties"));
        modelBuilder.AddOutboxMessageEntity(cfg => cfg.ToTable("OutboxMessage", "carsties"));
        modelBuilder.AddOutboxStateEntity(cfg => cfg.ToTable("OutboxState", "carsties"));
    }

    /// <summary>
    /// Configures global query filters for all entities implementing ISoftDeletable.
    /// This ensures soft-deleted entities are automatically excluded from queries.
    /// Use IgnoreQueryFilters() to include soft-deleted entities when needed.
    /// </summary>
    private static void ConfigureSoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        // Get all entity types that implement ISoftDeletable
        var softDeletableEntityTypes = modelBuilder
            .Model.GetEntityTypes()
            .Where(entityType => typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            .Select(entityType => entityType.ClrType)
            .ToList();

        foreach (var entityType in softDeletableEntityTypes)
        {
            // Create lambda expression: entity => !entity.IsDeleted
            var parameter = Expression.Parameter(entityType, "entity");
            var isDeletedProperty = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
            var notDeleted = Expression.Not(isDeletedProperty);
            var lambda = Expression.Lambda(notDeleted, parameter);

            // Apply the query filter
            modelBuilder.Entity(entityType).HasQueryFilter(lambda);
        }
    }
}
