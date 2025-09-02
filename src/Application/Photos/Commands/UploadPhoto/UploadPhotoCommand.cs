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
    IUserContext userContext
) : ICommandHandler<UploadPhotoCommand, PhotoDto>
{
    public async Task<Result<PhotoDto>> Handle(UploadPhotoCommand request, CancellationToken cancellationToken)
    {
        var member = await context
            .Members.Include(x => x.User)
            .Include(x => x.Photos)
            .SingleOrDefaultAsync(x => x.Id == userContext.UserId, cancellationToken);

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

        if (member.ImageUrl == null)
        {
            member.ImageUrl = photo.Url;
            member.User.ImageUrl = photo.Url;
        }

        member.Photos.Add(photo);
        await context.SaveChangesAsync(cancellationToken);

        var photoResult = new PhotoDto(photo.Id, result.SecureUrl.ToString(), result.PublicId, member.Id);

        return Result.Success(photoResult);
    }
}
