namespace CleanArch.Application.Auctions.CreateAuction;

public class CreateAuctionCommandValidator : AbstractValidator<CreateAuctionCommand>
{
    public CreateAuctionCommandValidator()
    {
        RuleFor(x => x.Make).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Year).GreaterThan(1900).LessThanOrEqualTo(DateTime.Now.Year + 1);
        RuleFor(x => x.Color).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Mileage).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ImageUrl).NotEmpty().Must(BeAValidUrl).WithMessage("Image URL must be a valid URL");
        RuleFor(x => x.ReservePrice).GreaterThan(0);
        RuleFor(x => x.AuctionEnd).GreaterThan(DateTime.UtcNow).WithMessage("Auction end date must be in the future");
    }

    private static bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
