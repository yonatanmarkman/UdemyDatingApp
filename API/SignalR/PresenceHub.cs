using System.Security.Claims;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class PresenceHub(PresenceTracker presenceTracker) : Hub
{
    public override async Task OnConnectedAsync()
    {
        await presenceTracker.UserConnected(GetUserId(), Context.ConnectionId);
        await Clients.Others.SendAsync(MethodNameConstants._UserOnlineMethodName, GetUserId());

        await MakeCallerNotifyTheCurrentUsers();

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await presenceTracker.UserDisconnected(GetUserId(), Context.ConnectionId);
        await Clients.Others.SendAsync(MethodNameConstants._UserOfflineMethodName, GetUserId());

        await MakeCallerNotifyTheCurrentUsers();
        
        await base.OnDisconnectedAsync(exception);
    }

    private async Task MakeCallerNotifyTheCurrentUsers()
    {
        string[] currentUsers = await presenceTracker.GetOnlineUsers();
        await Clients.Caller.SendAsync(MethodNameConstants._GetOnlineUsersMethodName, currentUsers);
    }

    private string GetUserId()
    {
        return Context.User?.GetMemberId()
            ?? throw new HubException("Cannot get member id");
    }
}
