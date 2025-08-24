using CleanArch.Application.Members.Commands.UpdateMember;
using CleanArch.Application.Members.Queries.GetMember;
using CleanArch.Application.Members.Queries.GetMemberPhotos;
using CleanArch.Application.Members.Queries.GetMembers;
using CleanArch.Web.Api.Extensions;

namespace CleanArch.Web.Api.Endpoints;

public class Members : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder
            .MapGet("/", GetMembers)
            .WithName("GetMembers")
            .WithSummary("Get all members")
            .WithDescription("Retrieves a list of all members with their photos")
            .Produces<List<MemberDto>>();

        groupBuilder
            .MapGet("/{id:guid}", GetMember)
            .WithName("GetMember")
            .WithSummary("Get member by ID")
            .WithDescription("Retrieves a specific member by their ID")
            .Produces<MemberDto>()
            .Produces(404);

        groupBuilder
            .MapGet("/{id:guid}/photos", GetMemberPhotos)
            .WithName("GetMemberPhotos")
            .WithSummary("Get member photos")
            .WithDescription("Retrieves all photos for a specific member")
            .Produces<List<PhotoDto>>();

        groupBuilder
            .MapPut("/", UpdateMember)
            .WithName("UpdateMember")
            .WithSummary("Update member profile")
            .WithDescription("Updates the current member's profile information")
            .Produces(204)
            .Produces(404);
    }

    private static async Task<IResult> GetMembers(IMediator mediator)
    {
        var result = await mediator.Send(new GetMembersQuery());
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    private static async Task<IResult> GetMember(Guid id, IMediator mediator)
    {
        var result = await mediator.Send(new GetMemberQuery(id));
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    private static async Task<IResult> GetMemberPhotos(Guid id, IMediator mediator)
    {
        var result = await mediator.Send(new GetMemberPhotosQuery(id));
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    private static async Task<IResult> UpdateMember(UpdateMemberCommand command, IMediator mediator)
    {
        var result = await mediator.Send(command);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
