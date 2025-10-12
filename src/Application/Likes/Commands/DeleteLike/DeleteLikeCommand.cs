namespace CleanArch.Application.Likes.Commands.DeleteLike;

public record DeleteLikeCommand(Guid TargetMemberId) : ICommand;
