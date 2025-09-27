using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Photos.DTOs;

namespace CleanArch.Application.Admin.GetPhotosForModeration;

public sealed record GetPhotosForModerationQuery : IQuery<List<PhotoDto>>;

public class GetPhotosForModerationQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetPhotosForModerationQuery, List<PhotoDto>>
{
    public async Task<Result<List<PhotoDto>>> Handle(
        GetPhotosForModerationQuery request,
        CancellationToken cancellationToken
    )
    {
        var photos = await context
            .Photos.AsNoTracking()
            .IgnoreQueryFilters() // Ignore the IsApproved filter to get unapproved photos
            .Where(p => !p.IsApproved)
            .ProjectToType<PhotoDto>()
            .ToListAsync(cancellationToken);

        return photos;
    }
}
