using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Photos.DTOs;
using CleanArch.Domain.Members;
using CleanArch.Domain.Photos;
using CloudinaryDotNet.Actions;

namespace CleanArch.Application.Photos.Commands.UploadPhoto;

public class UploadPhotoCommandHandler(
    IApplicationDbContext context,
    IPhotoService photoService,
    ICurrentUser userContext
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

        ImageUploadResult result = await photoService.UploadPhotoAsync(request.FileDto);

        if (result.Error != null)
        {
            return Result.Failure<PhotoDto>(Domain.Common.Error.Failure("Photo.UploadFailed", result.Error.Message));
        }

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,
            MemberId = member.Id,
            Member = member,
        };

        member.Photos.Add(photo);
        await context.SaveChangesAsync(cancellationToken);

        var photoResult = new PhotoDto(
            photo.Id,
            result.SecureUrl.ToString(),
            result.PublicId,
            member.Id,
            photo.IsApproved
        );
        return Result.Success(photoResult);
    }
}
