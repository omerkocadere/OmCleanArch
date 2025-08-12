using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Common.Models;
using CleanArch.Application.Members.Queries.GetMembers;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Application.Members.Queries.GetMemberPhotos;

public record GetMemberPhotosQuery(Guid MemberId) : IQuery<List<PhotoDto>>;

public class GetMemberPhotosQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetMemberPhotosQuery, List<PhotoDto>>
{
    public async Task<Result<List<PhotoDto>>> Handle(GetMemberPhotosQuery request, CancellationToken cancellationToken)
    {
        return await context
            .Members.Where(m => m.Id == request.MemberId)
            .SelectMany(m => m.Photos)
            .ProjectToType<PhotoDto>()
            .ToListAsync(cancellationToken);
    }
}
