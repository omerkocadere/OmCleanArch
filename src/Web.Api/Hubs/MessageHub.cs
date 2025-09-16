using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Messages.Commands.AddToGroup;
using CleanArch.Application.Messages.Commands.RemoveConnection;
using CleanArch.Application.Messages.Commands.SendMessageCommand;
using CleanArch.Application.Messages.Queries.GetMessageThread;
using CleanArch.Web.Api.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CleanArch.Web.Api.Hubs;

[Authorize]
public class MessageHub(
    IUserContext userContext,
    IHubContext<PresenceHub> presenceHub,
    IPresenceTracker presenceTracker,
    IMediator mediator
) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var otherUser =
            httpContext?.Request?.Query["userId"].ToString() ?? throw new HubException("Other user not found");

        if (!Guid.TryParse(otherUser, out var otherUserId))
            throw new HubException("Invalid user id");

        var currentUserId = GetUserId();
        var groupName = GetGroupName(currentUserId.ToString(), otherUser);

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var addToGroupResult = await mediator.Send(
            new AddToGroupCommand(groupName, Context.ConnectionId, currentUserId)
        );

        if (addToGroupResult.IsFailure)
            throw new HubException("Failed to add to group");

        // Get message thread using existing query (it gets current user from IUserContext)
        var threadResult = await mediator.Send(new GetMessageThreadQuery(otherUserId));
        if (threadResult.IsSuccess)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessageThread", threadResult.Value);
        }
    }

    public async Task SendMessage(CreateMessageRequest request)
    {
        var currentUserId = GetUserId();
        var groupName = GetGroupName(currentUserId.ToString(), request.RecipientId.ToString());

        // Send message with comprehensive command
        var command = new SendMessageCommand(request.RecipientId, request.Content, groupName);
        var result = await mediator.Send(command);

        if (result.IsFailure)
            throw new HubException("Cannot send message");

        var messageDto = result.Value;

        // Send to group (message is already marked as read for group members)
        await Clients.Group(groupName).SendAsync("NewMessage", messageDto);

        // Send notification to user if they're online but NOT in this group
        // If DateRead is not null, it means user was in group when message was sent
        var userInGroup = messageDto.DateRead.HasValue;

        var connections = await presenceTracker.GetConnectionsForUser(request.RecipientId.ToString());
        if (connections is { Count: > 0 } && !userInGroup)
        {
            await presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", messageDto);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await mediator.Send(new RemoveConnectionCommand(Context.ConnectionId));
        await base.OnDisconnectedAsync(exception);
    }

    private static string GetGroupName(string? caller, string? other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

    private Guid GetUserId()
    {
        return userContext.UserId ?? throw new HubException("Cannot get user id");
    }
}

public record CreateMessageRequest(Guid RecipientId, string Content);
