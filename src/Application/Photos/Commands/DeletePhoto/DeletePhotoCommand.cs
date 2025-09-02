using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Domain.Common;
using CleanArch.Domain.Photos;
using CloudinaryDotNet.Actions;

namespace CleanArch.Application.Photos.Commands.DeletePhoto;

public record DeletePhotoCommand(string PublicId) : ICommand;

public class DeletePhotoCommandHandler(IPhotoService photoService)
    : ICommandHandler<DeletePhotoCommand>
{
    public async Task<Result> Handle(DeletePhotoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            DeletionResult result = await photoService.DeletePhotoAsync(request.PublicId);

            if (result.Error != null)
            {
                return Result.Failure(PhotoErrors.DeleteFailed);
            }

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(PhotoErrors.DeleteError);
        }
    }
}
