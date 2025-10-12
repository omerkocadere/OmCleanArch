using CleanArch.Application.Common.Interfaces;
using CleanArch.Domain.Photos;

namespace CleanArch.Application.Admin.ApprovePhoto;

public class ApprovePhotoCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    : ICommandHandler<ApprovePhotoCommand>
{
    public async Task<Result> Handle(ApprovePhotoCommand request, CancellationToken cancellationToken)
    {
        var photo = await context
            .Photos.IgnoreQueryFilters() // Ignore the IsApproved filter to get unapproved photos
            .Include(p => p.Member)
            .SingleOrDefaultAsync(p => p.Id == request.PhotoId, cancellationToken);

        if (photo == null)
        {
            return Result.Failure(PhotoErrors.NotFound);
        }

        if (photo.IsApproved)
        {
            return Result.Failure(PhotoErrors.AlreadyApproved);
        }

        photo.IsApproved = true;

        // If member doesn't have a main image, set this as main
        if (photo.Member.ImageUrl == null)
        {
            // First update ApplicationUser through IIdentityService
            var updateResult = await identityService.UpdateUserAsync(photo.Member.Id, imageUrl: photo.Url);

            if (!updateResult.IsSuccess)
            {
                return Result.Failure(updateResult.Error);
            }

            // Then update Member ImageUrl
            photo.Member.ImageUrl = photo.Url;
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
