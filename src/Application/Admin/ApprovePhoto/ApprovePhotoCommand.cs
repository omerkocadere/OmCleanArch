namespace CleanArch.Application.Admin.ApprovePhoto;

public sealed record ApprovePhotoCommand(Guid PhotoId) : ICommand;
