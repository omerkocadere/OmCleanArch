using CleanArch.Domain.Common;

namespace CleanArch.Domain.Messages;

public static class MessageErrors
{
    public static Error NotFound(Guid messageId) =>
        Error.NotFound("Messages.NotFound", $"The message with the Id = '{messageId}' was not found");

    public static readonly Error NotFoundGeneral = Error.NotFound("Messages.NotFound", "The message was not found");

    public static readonly Error CannotDelete = Error.Forbidden(
        "Messages.CannotDelete",
        "You cannot delete this message"
    );

    public static readonly Error SelfMessage = Error.Problem("Messages.SelfMessage", "Cannot send message to yourself");
}
