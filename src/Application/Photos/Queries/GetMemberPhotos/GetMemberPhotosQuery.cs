using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Photos.DTOs;
using CleanArch.Domain.Common;

namespace CleanArch.Application.Photos.Queries.GetMemberPhotos;

public record GetMemberPhotosQuery(Guid MemberId, bool IsCurrentUser = false) : IQuery<List<PhotoDto>>;

public class GetMemberPhotosQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetMemberPhotosQuery, List<PhotoDto>>
{
    public async Task<Result<List<PhotoDto>>> Handle(GetMemberPhotosQuery request, CancellationToken cancellationToken)
    {
        var query = context
            .Members.Where(m => m.Id == request.MemberId)
            .SelectMany(m => m.Photos);

        // If current user is viewing their own photos, show all photos (including unapproved)
        if (request.IsCurrentUser)
        {
            query = query.IgnoreQueryFilters();
        }

        var photos = await query
            .ProjectToType<PhotoDto>()
            .ToListAsync(cancellationToken);

        return photos;
    }
}
