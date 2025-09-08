using FluentValidation;

namespace CleanArch.Application.Messages.Commands.CreateMessage;

public class CreateMessageCommandValidator : AbstractValidator<CreateMessageCommand>
{
    public CreateMessageCommandValidator()
    {
        RuleFor(x => x.RecipientId).NotEmpty().WithMessage("Recipient ID is required.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Message content is required.")
            .MaximumLength(500)
            .WithMessage("Message content must not exceed 500 characters.");
    }
}
