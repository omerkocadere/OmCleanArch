using Microsoft.EntityFrameworkCore;

namespace Dummy.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Configure all DateTime properties to use timestamp without time zone for PostgreSQL
        configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp without time zone");
        configurationBuilder.Properties<DateTime?>().HaveColumnType("timestamp without time zone");
    }
}
