using CleanArch.Domain.Auctions;
using CleanArch.Domain.Comments;
using CleanArch.Domain.Members;
using CleanArch.Domain.Photos;
using CleanArch.Domain.TodoItems;
using CleanArch.Domain.TodoLists;
using CleanArch.Domain.Users;

namespace CleanArch.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<TodoList> TodoLists { get; }
    DbSet<TodoItem> TodoItems { get; }
    DbSet<Auction> Auctions { get; }
    DbSet<Member> Members { get; }
    DbSet<Photo> Photos { get; }
    DbSet<Comment> Comments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
