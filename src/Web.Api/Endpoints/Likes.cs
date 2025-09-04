using CleanArch.Application.Common.Models;
using CleanArch.Application.Likes.Commands.ToggleLike;
using CleanArch.Application.Likes.Queries.GetCurrentMemberLikeIds;
using CleanArch.Application.Likes.Queries.GetMemberLikes;
using CleanArch.Application.Members.Queries.GetMembers;
using CleanArch.Web.Api.Extensions;

namespace CleanArch.Web.Api.Endpoints;

public class Likes : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorizationWithTracking();
        groupBuilder.MapPost(ToggleLike, "{targetMemberId:guid}").Produces(200).Produces(400).Produces(404);
        groupBuilder.MapGet(GetCurrentMemberLikeIds, "list").Produces<IReadOnlyList<Guid>>();
        groupBuilder.MapGet(GetMemberLikes).Produces<PaginatedList<MemberDto>>().Produces(400).Produces(404);
    }

    private static async Task<IResult> ToggleLike(Guid targetMemberId, IMediator mediator)
    {
        var result = await mediator.Send(new ToggleLikeCommand(targetMemberId));
        return result.Match(() => Results.Ok(), CustomResults.Problem);
    }

    private static async Task<IResult> GetCurrentMemberLikeIds(IMediator mediator)
    {
        var result = await mediator.Send(new GetCurrentMemberLikeIdsQuery());
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    private static async Task<IResult> GetMemberLikes([AsParameters] LikesParams likesParams, IMediator mediator)
    {
        var result = await mediator.Send(new GetMemberLikesQuery(likesParams));
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
