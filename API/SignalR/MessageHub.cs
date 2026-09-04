using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class MessageHub(
    IMessageRepository messageRepository,
    IMemberRepository memberRepository
    ) : Hub
{
    public override async Task OnConnectedAsync()
    {
        string currentUser = GetCurrentUserId();
        string otherUser = GetOtherUserId();

        string groupName = GetGroupName(currentUser, otherUser);

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        IReadOnlyList<MessageDto> messages
             = await messageRepository.GetMessageThreadAsync(currentUser, otherUser);

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
        Member? sender = await memberRepository
            .GetMemberByIdAsync(GetCurrentUserId());
        Member? recipient = await memberRepository
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

        messageRepository.AddMessage(message);

        if (await messageRepository.SaveAllAsync())
        {
            string group = GetGroupName(sender.Id, recipient.Id);
            await Clients.Group(group)
                .SendAsync(MethodNameConstants._NewMessage, message.ConvertToDto());
        }
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
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