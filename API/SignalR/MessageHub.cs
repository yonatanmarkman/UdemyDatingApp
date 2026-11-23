using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class MessageHub(IUnitOfWork unitOfWork,
    IHubContext<PresenceHub> presenceHub
    ) : Hub
{
    public override async Task OnConnectedAsync()
    {
        string currentUser = GetCurrentUserId();
        string otherUser = GetOtherUserId();

        string groupName = GetGroupName(currentUser, otherUser);

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await AddConnectionToGroup(groupName);

        IReadOnlyList<MessageDto> messages
             = await unitOfWork.MessageRepository.GetMessageThreadAsync(currentUser, otherUser);

        await Clients.Group(groupName)
            .SendAsync(MethodNameConstants._ReceiveMessageThread, messages);
    }
    
    private string GetCurrentUserId()
    {
        return Context.User?.GetMemberId()
            ?? throw new HubException("Current user not found. ");
    }

    private string GetOtherUserId()
    {
        HttpContext? httpContext = Context.GetHttpContext();
        return httpContext?.Request?.Query["userId"].ToString()
            ?? throw new HubException("Other user not found. ");
    }

    public async Task SendMessage(SendMessageDto sendMessageDto)
    {
        Member? sender = await unitOfWork.MemberRepository
            .GetMemberByIdAsync(GetCurrentUserId());
        Member? recipient = await unitOfWork.MemberRepository
            .GetMemberByIdAsync(sendMessageDto.RecipientId);

        if (recipient == null 
            || sender == null 
            || sender.Id == sendMessageDto.RecipientId)
        {
            throw new HubException("Cannot send message. ");
        }

        var message = new Message
        {
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            Content = sendMessageDto.Content
        };

        string groupName = GetGroupName(sender.Id, recipient.Id);
        Group? group = await unitOfWork.MessageRepository.GetMessageGroupAsync(groupName);
        bool recipientInGroup = group != null && group.Connections
                            .Any(conn => conn.UserId == message.RecipientId);

        // If the recipient is connected whilst the message is being sent,
        // then set the 'Date read' to be DateTime.UtcNow.
        if (recipientInGroup)
        {
            message.DateRead = DateTime.UtcNow;
        }

        unitOfWork.MessageRepository.AddMessage(message);

        if (await unitOfWork.Complete())
        {
            await Clients.Group(groupName)
                .SendAsync(MethodNameConstants._NewMessage, message.ConvertToDto());
            List<string> connections
                 = await PresenceTracker.GetConnectionsForUser(recipient.Id);

            // Notify the recipient about the new message,
            // if he's not connected to the group.
            if (connections != null && connections.Count > 0 && !recipientInGroup)
            {
                await presenceHub.Clients.Clients(connections)
                    .SendAsync(MethodNameConstants._NewMessageReceived, message.ConvertToDto());
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await unitOfWork.MessageRepository.RemoveConnectionAsync(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    private async Task<bool> AddConnectionToGroup(string groupName)
    {
        Group? group = await unitOfWork.MessageRepository.GetMessageGroupAsync(groupName);
        var connection = new Connection(Context.ConnectionId, GetCurrentUserId());

        if (group == null)
        {
            group = new Group(groupName);
            unitOfWork.MessageRepository.AddGroup(group);
        }

        group.Connections.Add(connection);

        return await unitOfWork.Complete();
    }

    /*
    Make sure that the order of the names is always the same,
    regardless of the two users' connection order.
    */
    private static string GetGroupName(string? caller, string? otherUser)
    {
        bool stringCompare = string.CompareOrdinal(caller, otherUser) < 0;
        return stringCompare ? $"{caller}-{otherUser}" : $"{otherUser}-{caller}";
    }

}