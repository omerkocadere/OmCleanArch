namespace CleanArch.Application.Members.Commands.UpdateMember;

public class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberCommandValidator()
    {
        RuleFor(c => c.DisplayName).MaximumLength(200).When(c => c.DisplayName is not null);

        RuleFor(c => c.Description).MaximumLength(1000).When(c => c.Description is not null);

        RuleFor(c => c.City).MaximumLength(100).When(c => c.City is not null);

        RuleFor(c => c.Country).MaximumLength(100).When(c => c.Country is not null);
    }
}
