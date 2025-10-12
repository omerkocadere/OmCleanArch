namespace CleanArch.Application.Messages.Commands.RemoveConnection;

public record RemoveConnectionCommand(string ConnectionId) : ICommand;
