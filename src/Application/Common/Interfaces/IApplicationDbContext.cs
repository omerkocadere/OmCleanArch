using CleanArch.Domain.Auctions;
using CleanArch.Domain.Members;
using CleanArch.Domain.Messages;
using CleanArch.Domain.Photos;
using CleanArch.Domain.TodoItems;
using CleanArch.Domain.TodoLists;
using Microsoft.EntityFrameworkCore.Storage;

namespace CleanArch.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Note: User management is now handled through IIdentityService
    // ApplicationUser is in Infrastructure layer following Clean Architecture
    DbSet<TodoList> TodoLists { get; }
    DbSet<TodoItem> TodoItems { get; }
    DbSet<Auction> Auctions { get; }
    DbSet<Member> Members { get; }
    DbSet<Photo> Photos { get; }
    DbSet<MemberLike> Likes { get; }
    DbSet<Message> Messages { get; }


    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
