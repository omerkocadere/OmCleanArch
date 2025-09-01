namespace CleanArch.Application.Auctions.Update;

public class UpdateAuctionCommandValidator : AbstractValidator<UpdateAuctionCommand>
{
    public UpdateAuctionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Make).MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Make));
        RuleFor(x => x.Model).MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Model));
        RuleFor(x => x.Year).GreaterThan(1900).LessThanOrEqualTo(DateTime.Now.Year + 1).When(x => x.Year.HasValue);
        RuleFor(x => x.Color).MaximumLength(30).When(x => !string.IsNullOrEmpty(x.Color));
        RuleFor(x => x.Mileage).GreaterThanOrEqualTo(0).When(x => x.Mileage.HasValue);
    }
}
