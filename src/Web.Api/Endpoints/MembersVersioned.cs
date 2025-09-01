using CleanArch.Application.Common.Behaviours;
using CleanArch.Application.Common.Security;
using CleanArch.Application.Members.Queries.GetMembers;
using CleanArch.Web.Api.Extensions;

namespace CleanArch.Web.Api.Endpoints;

public class MembersVersioned : EndpointGroupBase, IVersionedEndpointGroup
{
    public override string GroupName => "Members";

    public static int[] SupportedVersions => [1, 2];

    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder
            .MapGet("", GetMembers)
            .HasPermission(PermissionNames.ReadMember)
            .MapToApiVersion(1)
            .Produces<List<MemberDto>>();
        groupBuilder
            .MapGet("", GetMembersV2)
            .HasPermission(PermissionNames.ReadMember)
            .MapToApiVersion(2)
            .Produces<List<MemberDto>>();
    }

    private static async Task<IResult> GetMembers([AsParameters] GetMembersQuery query, IMediator mediator)
    {
        var result = await mediator.Send(query);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    private static async Task<IResult> GetMembersV2([AsParameters] GetMembersQuery query, IMediator mediator)
    {
        var result = await mediator.Send(query);
        return result.Match(
            members =>
                Results.Ok(
                    new
                    {
                        Data = members,
                        Version = "2.0",
                        members.Count,
                    }
                ),
            CustomResults.Problem
        );
    }
}
