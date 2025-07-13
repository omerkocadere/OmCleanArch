using CleanArch.Application.Auctions.CreateAuction;
using CleanArch.Application.Auctions.DeleteAuction;
using CleanArch.Application.Auctions.DTOs;
using CleanArch.Application.Auctions.GetAuctionById;
using CleanArch.Application.Auctions.GetAuctions;
using CleanArch.Application.Auctions.UpdateAuction;
using CleanArch.Application.Common.Models;
using CleanArch.Web.Api.Extensions;

namespace CleanArch.Web.Api.Endpoints;

public class Auctions : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(GetAuctions)
            .MapGet(GetAuctionById, "{id:guid}")
            .MapPost(CreateAuction)
            .MapPut(UpdateAuction, "{id:guid}")
            .MapDelete(DeleteAuction, "{id:guid}");
    }

    public async Task<IResult> GetAuctions(ISender sender, DateTime? date = null)
    {
        var result = await sender.Send(new GetAuctionsQuery(date));
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    public async Task<IResult> GetAuctionById(ISender sender, Guid id)
    {
        var result = await sender.Send(new GetAuctionByIdQuery(id));
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    public async Task<IResult> CreateAuction(ISender sender, CreateAuctionCommand command)
    {
        Result<AuctionDto> result = await sender.Send(command);

        return result.Match(
            dto => Results.CreatedAtRoute(nameof(GetAuctionById), new { id = dto.Id }, dto),
            CustomResults.Problem
        );
    }

    public async Task<IResult> UpdateAuction(ISender sender, Guid id, UpdateAuctionCommand command)
    {
        if (id != command.Id)
            return TypedResults.BadRequest("Id mismatch between route and body");

        Result<AuctionDto> result = await sender.Send(command);

        return result.Match(Results.Ok, CustomResults.Problem);
    }

    public async Task<IResult> DeleteAuction(ISender sender, Guid id)
    {
        Result result = await sender.Send(new DeleteAuctionCommand(id));
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
