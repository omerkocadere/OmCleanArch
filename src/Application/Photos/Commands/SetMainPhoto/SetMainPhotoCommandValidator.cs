namespace CleanArch.Application.Photos.Commands.SetMainPhoto;

public class SetMainPhotoCommandValidator : AbstractValidator<SetMainPhotoCommand>
{
    public SetMainPhotoCommandValidator()
    {
        RuleFor(x => x.PhotoId).NotEmpty().WithMessage("Photo ID is required.");
    }
}
