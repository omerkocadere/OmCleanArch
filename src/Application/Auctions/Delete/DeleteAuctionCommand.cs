namespace CleanArch.Application.Auctions.Delete;

public record DeleteAuctionCommand(Guid Id) : ICommand;
