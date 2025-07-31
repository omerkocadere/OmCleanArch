namespace CleanArch.Domain.Auctions;

public static class AuctionErrors
{
    public static Error NotFound(Guid auctionId) =>
        Error.NotFound("Auctions.NotFound", $"The auction with Id = '{auctionId}' was not found");
}
