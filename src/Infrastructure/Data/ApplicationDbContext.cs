using System.Reflection;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Domain.Auctions;
using CleanArch.Domain.Products;
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
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Auction> Auctions => Set<Auction>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<OutboxMessageConsumer> OutboxMessageConsumers => Set<OutboxMessageConsumer>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Add MassTransit outbox tables under auction schema
        builder.AddInboxStateEntity(cfg => cfg.ToTable("InboxState", "carsties"));
        builder.AddOutboxMessageEntity(cfg => cfg.ToTable("OutboxMessage", "carsties"));
        builder.AddOutboxStateEntity(cfg => cfg.ToTable("OutboxState", "carsties"));
    }
}
