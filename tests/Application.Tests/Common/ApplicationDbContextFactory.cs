using Microsoft.EntityFrameworkCore;

namespace CleanArch.Application.Tests.Common;

public static class ApplicationDbContextFactory
{
    public static Mock<IApplicationDbContext> CreateMockContext()
    {
        var context = new Mock<IApplicationDbContext>();

        // Create in-memory DbSets
        var users = CreateDbSet<User>();
        var todoLists = CreateDbSet<CleanArch.Domain.TodoLists.TodoList>();
        var todoItems = CreateDbSet<CleanArch.Domain.TodoItems.TodoItem>();

        context.Setup(x => x.Users).Returns(users.Object);
        context.Setup(x => x.TodoLists).Returns(todoLists.Object);
        context.Setup(x => x.TodoItems).Returns(todoItems.Object);

        context.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return context;
    }

    private static Mock<DbSet<T>> CreateDbSet<T>()
        where T : class
    {
        var data = new List<T>().AsQueryable();

        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

        // Setup Add method
        mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(item => { });

        return mockSet;
    }
}
