namespace CleanArch.Domain.Auctions;

public static class AuctionErrors
{
    public static Error NotFound(int auctionId) =>
        Error.NotFound("Auctions.NotFound", $"The auction with Id = '{auctionId}' was not found");
}
