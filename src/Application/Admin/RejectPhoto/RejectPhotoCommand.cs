namespace CleanArch.Application.Admin.RejectPhoto;

public sealed record RejectPhotoCommand(Guid PhotoId) : ICommand;
