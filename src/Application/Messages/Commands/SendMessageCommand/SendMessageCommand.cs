using CleanArch.Application.Messages.Queries.Common;

namespace CleanArch.Application.Messages.Commands.SendMessageCommand;

public record SendMessageCommand(Guid RecipientId, string Content, string GroupName) : ICommand<MessageDto>;
