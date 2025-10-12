namespace CleanArch.Application.Members.Commands.UpdateUserActivity;

public record UpdateUserActivityCommand(Guid UserId) : ICommand;
