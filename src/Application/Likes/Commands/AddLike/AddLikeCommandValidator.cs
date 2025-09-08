using FluentValidation;

namespace CleanArch.Application.Likes.Commands.AddLike;

public class AddLikeCommandValidator : AbstractValidator<AddLikeCommand>
{
    public AddLikeCommandValidator()
    {
        RuleFor(x => x.TargetMemberId).NotEmpty().WithMessage("Target member ID is required.");
    }
}
