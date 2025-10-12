namespace CleanArch.Application.Messages.Commands.AddToGroup;

public record AddToGroupCommand(string GroupName, string ConnectionId, Guid UserId) : ICommand<bool>;
