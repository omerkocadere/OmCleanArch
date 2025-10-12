using CleanArch.Application.Common.Interfaces;
using CleanArch.Domain.Photos;

namespace CleanArch.Application.Admin.RejectPhoto;

public class RejectPhotoCommandHandler(IApplicationDbContext context, IPhotoService photoService)
    : ICommandHandler<RejectPhotoCommand>
{
    public async Task<Result> Handle(RejectPhotoCommand request, CancellationToken cancellationToken)
    {
        var photo = await context
            .Photos.IgnoreQueryFilters() // Ignore the IsApproved filter to get unapproved photos
            .SingleOrDefaultAsync(p => p.Id == request.PhotoId, cancellationToken);

        if (photo == null)
        {
            return Result.Failure(PhotoErrors.NotFound);
        }

        // Delete from external service if it has a PublicId
        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);

            if (result.Error != null)
            {
                return Result.Failure(Domain.Common.Error.Failure("Photo.DeleteFailed", result.Error.Message));
            }
        }

        // Remove from database
        context.Photos.Remove(photo);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
