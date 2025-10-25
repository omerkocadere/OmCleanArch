using CleanArch.Application.Common.Interfaces;
using CleanArch.Domain.Members;
using CleanArch.Domain.Photos;

namespace CleanArch.Application.Photos.Commands.SetMainPhoto;

public class SetMainPhotoCommandHandler(
    IApplicationDbContext context,
    ICurrentUser userContext,
    IIdentityService identityService
) : ICommandHandler<SetMainPhotoCommand>
{
    public async Task<Result> Handle(SetMainPhotoCommand request, CancellationToken cancellationToken)
    {
        // Validate user context first
        if (userContext.UserId == null)
        {
            return Result.Failure(MemberErrors.NotFound);
        }

        var userId = userContext.UserId.Value;

        // Start atomic transaction
        using var transaction = await context.BeginTransactionAsync(cancellationToken);

        // Load member with tracking for updates
        var member = await context
            .Members.Include(x => x.Photos)
            .IgnoreQueryFilters() // Include unapproved photos to check approval status
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (member is null)
        {
            return Result.Failure(MemberErrors.NotFound);
        }

        var photo = member.Photos.SingleOrDefault(x => x.Id == request.PhotoId);

        if (photo is null || member.ImageUrl == photo.Url)
        {
            return Result.Failure(PhotoErrors.CannotSetMain);
        }

        // Only approved photos can be set as main
        if (!photo.IsApproved)
        {
            return Result.Failure(PhotoErrors.PhotoNotApproved);
        }

        // Set the new main photo
        member.ImageUrl = photo.Url;

        // Update ApplicationUser ImageUrl through IIdentityService
        var updateResult = await identityService.UpdateUserAsync(userId, imageUrl: photo.Url);

        if (!updateResult.IsSuccess)
        {
            return updateResult;
        }

        context.Members.Update(member);
        await context.SaveChangesAsync(cancellationToken);

        // Commit transaction - all operations successful
        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
