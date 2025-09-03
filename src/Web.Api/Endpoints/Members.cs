using CleanArch.Application.Members.Commands.UpdateMember;
using CleanArch.Application.Members.Queries.GetMember;
using CleanArch.Application.Members.Queries.GetMembers;
using CleanArch.Web.Api.Extensions;

namespace CleanArch.Web.Api.Endpoints;

public class Members : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.AddUserActivityTracking();

        groupBuilder.MapGet("/{id:guid}", GetMember).Produces<MemberDto>().Produces(404);
        groupBuilder.MapPut("/", UpdateMember).Produces(204).Produces(404);
    }

    private static async Task<IResult> GetMember(Guid id, IMediator mediator)
    {
        var result = await mediator.Send(new GetMemberByIdQuery(id));
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    private static async Task<IResult> UpdateMember(UpdateMemberCommand command, IMediator mediator)
    {
        var result = await mediator.Send(command);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
