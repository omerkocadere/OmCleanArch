using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Domain.Common;
using CleanArch.Domain.Members;
using CleanArch.Domain.Photos;
using CloudinaryDotNet.Actions;

namespace CleanArch.Application.Photos.Commands.UploadPhoto;

public record UploadPhotoCommand(FileDto FileDto) : ICommand<PhotoDto>;

public class UploadPhotoCommandHandler(
    IApplicationDbContext context,
    IPhotoService photoService,
    IUserContext userContext,
    IIdentityService identityService
) : ICommandHandler<UploadPhotoCommand, PhotoDto>
{
    public async Task<Result<PhotoDto>> Handle(UploadPhotoCommand request, CancellationToken cancellationToken)
    {
        // Validate user context first
        if (userContext.UserId == null)
        {
            return Result.Failure<PhotoDto>(MemberErrors.NotFound);
        }

        var userId = userContext.UserId.Value;

        // Load member with tracking for updates
        var member = await context
            .Members.Include(x => x.Photos)
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (member is null)
        {
            return Result.Failure<PhotoDto>(MemberErrors.NotFound);
        }

        // Step 1: Upload to external service (Cloudinary) first
        ImageUploadResult result = await photoService.UploadPhotoAsync(request.FileDto);

        if (result.Error != null)
        {
            return Result.Failure<PhotoDto>(Domain.Common.Error.Failure("Photo.UploadFailed", result.Error.Message));
        }

        // Step 2: Start atomic transaction for database operations
        using var transaction = await context.BeginTransactionAsync(cancellationToken);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,
            MemberId = member.Id,
            Member = member,
        };

        // If this is the first photo, set it as main
        if (member.ImageUrl == null)
        {
            member.ImageUrl = photo.Url;

            // Update ApplicationUser ImageUrl through IIdentityService
            var updateResult = await identityService.UpdateUserAsync(userId, imageUrl: photo.Url);

            if (!updateResult.IsSuccess)
            {
                // If identity update fails, we need to cleanup the uploaded photo
                await photoService.DeletePhotoAsync(result.PublicId);
                return Result.Failure<PhotoDto>(updateResult.Error);
            }
        }

        member.Photos.Add(photo);
        await context.SaveChangesAsync(cancellationToken);

        // Commit transaction - all database operations successful
        await transaction.CommitAsync(cancellationToken);

        var photoResult = new PhotoDto(photo.Id, result.SecureUrl.ToString(), result.PublicId, member.Id);
        return Result.Success(photoResult);
    }
}
