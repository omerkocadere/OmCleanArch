namespace CleanArch.Application.Likes.Commands.ToggleLike;

public record ToggleLikeCommand(Guid TargetMemberId) : ICommand;
