namespace CleanArch.Application.Messages.Commands.DeleteMessage;

public record DeleteMessageCommand(Guid MessageId) : ICommand;
