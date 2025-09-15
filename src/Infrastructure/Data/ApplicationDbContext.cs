using System.Linq.Expressions;
using System.Reflection;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Domain.Auctions;
using CleanArch.Domain.Common;
using CleanArch.Domain.Members;
using CleanArch.Domain.Messages;
using CleanArch.Domain.Photos;
using CleanArch.Domain.TodoItems;
using CleanArch.Domain.TodoLists;
using CleanArch.Infrastructure.BackgroundJobs.Outbox;
using CleanArch.Infrastructure.Identity;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CleanArch.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options),
        IApplicationDbContext
{
    public DbSet<TodoList> TodoLists => Set<TodoList>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<Auction> Auctions => Set<Auction>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<MemberLike> Likes => Set<MemberLike>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Connection> Connections => Set<Connection>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<OutboxMessageConsumer> OutboxMessageConsumers => Set<OutboxMessageConsumer>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configure global query filters for soft delete
        // This automatically excludes soft-deleted entities from all queries
        ConfigureSoftDeleteQueryFilters(builder); // Add MassTransit outbox tables under auction schema
        builder.AddInboxStateEntity(cfg => cfg.ToTable("InboxState", "carsties"));
        builder.AddOutboxMessageEntity(cfg => cfg.ToTable("OutboxMessage", "carsties"));
        builder.AddOutboxStateEntity(cfg => cfg.ToTable("OutboxState", "carsties"));
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

    // Interface implementation - delegates to Database.BeginTransactionAsync
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Database.BeginTransactionAsync(cancellationToken);
    }
}
