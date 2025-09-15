using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Web.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CleanArch.Web.Api.Hubs;

[Authorize]
public class PresenceHub(IPresenceTracker presenceTracker, IUserContext userContext) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        await presenceTracker.UserConnected(userId, Context.ConnectionId);
        await Clients.Others.SendAsync("UserOnline", userId);

        var currentUsers = await presenceTracker.GetOnlineUsers();
        await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        await presenceTracker.UserDisconnected(userId, Context.ConnectionId);
        await Clients.Others.SendAsync("UserOffline", userId);

        var currentUsers = await presenceTracker.GetOnlineUsers();
        await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);

        await base.OnDisconnectedAsync(exception);
    }

    private string GetUserId()
    {
        return userContext.UserId?.ToString() ?? throw new HubException("Cannot get user id");
    }
}
