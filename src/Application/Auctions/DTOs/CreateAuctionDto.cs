using System;
using System.ComponentModel.DataAnnotations;

namespace CleanArch.Application.Auctions.DTOs;

public class CreateAuctionDto
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int ReservePrice { get; set; }
    public DateTime AuctionEnd { get; set; }
}
