namespace CleanArch.Application.Likes.Commands.AddLike;

public record AddLikeCommand(Guid TargetMemberId) : ICommand;
