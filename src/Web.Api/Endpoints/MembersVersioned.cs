using CleanArch.Application.Common.Behaviours;
using CleanArch.Application.Common.Models;
using CleanArch.Application.Common.Security;
using CleanArch.Application.Members.Queries.GetMembers;
using CleanArch.Domain.Constants;
using CleanArch.Domain.Users;
using CleanArch.Web.Api.Extensions;

namespace CleanArch.Web.Api.Endpoints;

public class MembersVersioned : EndpointGroupBase, IVersionedEndpointGroup
{
    public override string GroupName => "Members";

    public static int[] SupportedVersions => [1, 2];

    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet("", GetMembers).MapToApiVersion(1).Produces<PaginatedList<MemberDto>>();
        groupBuilder.MapGet("", GetMembersV2).MapToApiVersion(2).Produces<PaginatedList<MemberDto>>();
    }

    private static async Task<IResult> GetMembers(IMediator mediator, [AsParameters] MemberParams memberParams)
    {
        var result = await mediator.Send(new GetMembersQuery(memberParams));
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    private static async Task<IResult> GetMembersV2(IMediator mediator, [AsParameters] MemberParams memberParams)
    {
        var result = await mediator.Send(new GetMembersQuery(memberParams));

        return result.Match(
            members =>
                Results.Ok(
                    new
                    {
                        Data = members.Items,
                        Version = "2.0",
                        Count = members.TotalCount,
                        members.PageNumber,
                        members.TotalPages,
                        members.HasNextPage,
                        members.HasPreviousPage,
                        Filters = new
                        {
                            memberParams.Gender,
                            MinAge = memberParams.MinAgeValue,
                            MaxAge = memberParams.MaxAgeValue,
                            OrderBy = memberParams.OrderByValue,
                        },
                    }
                ),
            CustomResults.Problem
        );
    }
}
