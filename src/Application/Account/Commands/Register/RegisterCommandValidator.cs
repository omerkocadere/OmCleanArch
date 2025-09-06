using FluentValidation;

namespace CleanArch.Application.Account.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be valid.")
            .MaximumLength(320)
            .WithMessage("Email must not exceed 320 characters.");

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("Display name is required.")
            .MaximumLength(100)
            .WithMessage("Display name must not exceed 100 characters.");

        // RuleFor(x => x.FirstName)
        //     .NotEmpty()
        //     .WithMessage("First name is required.")
        //     .MaximumLength(50)
        //     .WithMessage("First name must not exceed 50 characters.");

        // RuleFor(x => x.LastName)
        //     .NotEmpty()
        //     .WithMessage("Last name is required.")
        //     .MaximumLength(50)
        //     .WithMessage("Last name must not exceed 50 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters.");

        RuleFor(x => x.Gender).NotEmpty().WithMessage("Gender is required.");
        RuleFor(x => x.City).NotEmpty().WithMessage("City is required.");
        RuleFor(x => x.Country).NotEmpty().WithMessage("Country is required.");
        RuleFor(x => x.DateOfBirth).Must(BeValidAge).WithMessage("You must be at least 18 years old.");
    }

    private static bool BeValidAge(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age))
            age--;
        return age >= 18;
    }
}
