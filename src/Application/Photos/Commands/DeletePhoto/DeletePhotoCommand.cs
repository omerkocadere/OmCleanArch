using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Domain.Common;
using CleanArch.Domain.Members;
using CleanArch.Domain.Photos;
using CloudinaryDotNet.Actions;

namespace CleanArch.Application.Photos.Commands.DeletePhoto;

public record DeletePhotoCommand(Guid PhotoId) : ICommand;

public class DeletePhotoCommandHandler(
    IApplicationDbContext context,
    IPhotoService photoService,
    IUserContext userContext
) : ICommandHandler<DeletePhotoCommand>
{
    public async Task<Result> Handle(DeletePhotoCommand request, CancellationToken cancellationToken)
    {
        var member = await context
            .Members.Include(x => x.User)
            .Include(x => x.Photos)
            .SingleOrDefaultAsync(x => x.Id == userContext.UserId, cancellationToken);

        if (member is null)
        {
            return Result.Failure(MemberErrors.NotFound);
        }

        var photo = member.Photos.SingleOrDefault(x => x.Id == request.PhotoId);

        if (photo == null || photo.Url == member.ImageUrl)
        {
            return Result.Failure(PhotoErrors.CannotDelete);
        }

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null)
            {
                return Result.Failure(Domain.Common.Error.Failure("Photo.DeleteFailed", result.Error.Message));
            }
        }

        member.Photos.Remove(photo);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
