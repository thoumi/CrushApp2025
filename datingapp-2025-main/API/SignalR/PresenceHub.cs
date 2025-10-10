using System;
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
        try
        {
            var userId = GetUserId();
            await presenceTracker.UserConnected(userId, Context.ConnectionId);
            await Clients.Others.SendAsync("UserOnline", userId);

            var currentUsers = await presenceTracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
            
            Console.WriteLine($"User {userId} connected to PresenceHub");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in PresenceHub OnConnectedAsync: {ex.Message}");
            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userId = GetUserId();
            await presenceTracker.UserDisconnected(userId, Context.ConnectionId);
            await Clients.Others.SendAsync("UserOffline", userId);

            var currentUsers = await presenceTracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
            
            Console.WriteLine($"User {userId} disconnected from PresenceHub");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in PresenceHub OnDisconnectedAsync: {ex.Message}");
        }
        finally
        {
            await base.OnDisconnectedAsync(exception);
        }
    }

    private string GetUserId()
    {
        return Context.User?.GetMemberId()
            ?? throw new HubException("Cannot get member id");
    }
}
