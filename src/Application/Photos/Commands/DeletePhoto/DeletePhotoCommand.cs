namespace CleanArch.Application.Photos.Commands.DeletePhoto;

public record DeletePhotoCommand(Guid PhotoId) : ICommand;
