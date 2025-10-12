namespace CleanArch.Application.Photos.Commands.SetMainPhoto;

public record SetMainPhotoCommand(Guid PhotoId) : ICommand;
