using CleanArch.Application.Common.Models;
using CleanArch.Application.Messages.Commands.CreateMessage;
using CleanArch.Application.Messages.Commands.DeleteMessage;
using CleanArch.Application.Messages.Queries.Common;
using CleanArch.Application.Messages.Queries.GetMessages;
using CleanArch.Application.Messages.Queries.GetMessageThread;
using CleanArch.Web.Api.Common;
using CleanArch.Web.Api.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace CleanArch.Web.Api.Endpoints;

public class Messages : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorizationWithTracking();
        groupBuilder.MapPost(CreateMessage).Produces<MessageDto>(201).Produces(400).Produces(404);
        groupBuilder.MapGet(GetMessages).Produces<PaginatedList<MessageDto>>().Produces(400).Produces(404);
        groupBuilder
            .MapGet(GetMessageThread, "thread/{recipientId:guid}")
            .Produces<IReadOnlyList<MessageDto>>()
            .Produces(400)
            .Produces(404);
        groupBuilder.MapDelete(DeleteMessage, "{id:guid}").Produces(200).Produces(400).Produces(403).Produces(404);
    }

    private static async Task<IResult> CreateMessage(CreateMessageCommand command, IMediator mediator)
    {
        var result = await mediator.Send(command);
        return result.Match(messageDto => Results.Created(string.Empty, messageDto), CustomResults.Problem);
    }

    private static async Task<IResult> GetMessages([AsParameters] MessageParams messageParams, IMediator mediator)
    {
        var result = await mediator.Send(new GetMessagesQuery(messageParams));
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    private static async Task<IResult> GetMessageThread(Guid recipientId, IMediator mediator)
    {
        var result = await mediator.Send(new GetMessageThreadQuery(recipientId));
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    private static async Task<IResult> DeleteMessage(Guid id, IMediator mediator)
    {
        var result = await mediator.Send(new DeleteMessageCommand(id));
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
