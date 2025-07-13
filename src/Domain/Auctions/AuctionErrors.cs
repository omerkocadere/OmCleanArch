namespace CleanArch.Domain.Auctions;

public static class AuctionErrors
{
    public static Error NotFound(Guid auctionId) =>
        Error.NotFound("Auctions.NotFound", $"The auction with Id = '{auctionId}' was not found");

    public static Error UpdateFailed(Guid auctionId) =>
        Error.Problem("Auctions.UpdateFailed", $"Failed to update auction with Id = '{auctionId}'");

    public static Error DeleteFailed(Guid auctionId) =>
        Error.Problem("Auctions.DeleteFailed", $"Failed to delete auction with Id = '{auctionId}'");

    public static Error CreateFailed() => Error.Problem("Auctions.CreateFailed", "Failed to create auction");
}
