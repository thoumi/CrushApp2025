using System;
using API.Api.Extensions;
using API.Application.DTOs;
using API.Application.Interfaces;
using API.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;

namespace API.Api.SignalR;

[Authorize]
public class MessageHub(IMessagingUnitOfWork uow, IHubContext<PresenceHub> presenceHub) : Hub
{
    public override async Task OnConnectedAsync()
    {
        try
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext?.Request?.Query["userId"].ToString()
                ?? throw new HubException("Other user not found");
            var userId = GetUserId();
            var groupName = GetGroupName(userId, otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await AddToGroup(groupName);

            var messages = await uow.MessageRepository.GetMessageThread(userId, otherUser);

            await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
            
            Console.WriteLine($"User {userId} connected to MessageHub for conversation with {otherUser}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in MessageHub OnConnectedAsync: {ex.Message}");
            throw;
        }
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var sender = await uow.MemberRepository.GetMemberByIdAsync(GetUserId());
        var recipient = await uow.MemberRepository.GetMemberByIdAsync(createMessageDto.RecipientId);

        if (recipient == null || sender == null || sender.Id == createMessageDto.RecipientId)
            throw new HubException("Cannot send message");

        var message = new Message
        {
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            Content = createMessageDto.Content
        };

        var groupName = GetGroupName(sender.Id, recipient.Id);
        var group = await uow.MessageRepository.GetMessageGroup(groupName);
        var userInGroup = group != null && group.Connections.Any(x =>
             x.UserId == message.RecipientId);

        if (userInGroup)
        {
            message.DateRead = DateTime.UtcNow;
        }

        uow.MessageRepository.AddMessage(message);

        if (await uow.Complete())
        {
            await Clients.Group(groupName).SendAsync("NewMessage", message.ToDto());
            var connections = await PresenceTracker.GetConnectionsForUser(recipient.Id);
            if (connections != null && connections.Count > 0 && !userInGroup)
            {
                await presenceHub.Clients.Clients(connections)
                    .SendAsync("NewMessageReceived", message.ToDto());
            }
        }
    }

    public async Task SendTypingIndicator(string recipientId, bool isTyping)
    {
        var senderId = GetUserId();
        var groupName = GetGroupName(senderId, recipientId);
        
        if (isTyping)
        {
            // Récupérer les informations de l'utilisateur qui tape
            var sender = await uow.MemberRepository.GetMemberByIdAsync(senderId);
            if (sender != null)
            {
                var userInfo = new
                {
                    displayName = sender.DisplayName,
                    //imageUrl = sender.Photos?.FirstOrDefault(p => p.IsMain)?.Url ?? "/user.png"
                };
                                
                // Envoyer l'indicateur de typing avec les informations de l'utilisateur
                await Clients.OthersInGroup(groupName).SendAsync("UserTyping", senderId, isTyping, userInfo);
            }
            else
            {
                Console.WriteLine($"Sender not found for ID: {senderId}");
            }
        }
        else
        {
            // Pour arrêter le typing, pas besoin des informations utilisateur
            await Clients.OthersInGroup(groupName).SendAsync("UserTyping", senderId, isTyping);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await uow.MessageRepository.RemoveConnection(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    private async Task<bool> AddToGroup(string groupName)
    {
        var group = await uow.MessageRepository.GetMessageGroup(groupName);
        var connection = new Connection(Context.ConnectionId, GetUserId());

        if (group == null)
        {
            group = new Group(groupName);
            uow.MessageRepository.AddGroup(group);
        }

        group.Connections.Add(connection);

        return await uow.Complete();
    }

    private static string GetGroupName(string? caller, string? other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

    private string GetUserId()
    {
        return Context.User?.GetMemberId()
            ?? throw new HubException("Cannot get member id");
    }
}
