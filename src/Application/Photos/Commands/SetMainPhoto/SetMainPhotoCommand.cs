using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Domain.Common;
using CleanArch.Domain.Members;
using CleanArch.Domain.Photos;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Application.Photos.Commands.SetMainPhoto;

public record SetMainPhotoCommand(Guid PhotoId) : ICommand;

public class SetMainPhotoCommandHandler(IApplicationDbContext context, IUserContext userContext)
    : ICommandHandler<SetMainPhotoCommand>
{
    public async Task<Result> Handle(SetMainPhotoCommand request, CancellationToken cancellationToken)
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

        if (photo is null || member.ImageUrl == photo.Url)
        {
            return Result.Failure(PhotoErrors.CannotSetMain);
        }

        // Set the new main photo
        member.ImageUrl = photo.Url;
        member.User.ImageUrl = photo.Url;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
