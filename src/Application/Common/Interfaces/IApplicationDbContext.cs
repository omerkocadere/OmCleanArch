using CleanArch.Domain.Auctions;
using CleanArch.Domain.Members;
using CleanArch.Domain.Messages;
using CleanArch.Domain.Permissions;
using CleanArch.Domain.Photos;
using CleanArch.Domain.TodoItems;
using CleanArch.Domain.TodoLists;
using CleanArch.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace CleanArch.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<IdentityRole<Guid>> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<TodoList> TodoLists { get; }
    DbSet<TodoItem> TodoItems { get; }
    DbSet<Auction> Auctions { get; }
    DbSet<Member> Members { get; }
    DbSet<Photo> Photos { get; }
    DbSet<MemberLike> Likes { get; }
    DbSet<Message> Messages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
