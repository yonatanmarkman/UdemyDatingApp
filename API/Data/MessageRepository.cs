using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace API.Data;

public class MessageRepository(AppDbContext context) : IMessageRepository
{
    public bool AddMessage(Message message)
    {
        EntityEntry<Message> result = context.Messages.Add(message);
        return result.State == EntityState.Added;
    }

    public bool DeleteMessage(Message message)
    {
        EntityEntry<Message> result = context.Messages.Remove(message);
        return result.State == EntityState.Deleted;
    }

    public async Task<Message?> GetMessageAsync(string messageId)
    {
        return await context.Messages.FindAsync(messageId);
    }

    public async Task<PaginatedResult<MessageDto>> GetMessagesForMemberAsync(
        MessageParams messageParams)
    {
        IQueryable<Message> query = context.Messages
                                .OrderByDescending(m => m.MessageSent)
                                .AsQueryable();
        
        query = messageParams.Container switch
        {
            "Outbox" => query.Where(m => m.SenderId == messageParams.MemberId),
            _ => query.Where(m => m.RecipientId == messageParams.MemberId)
        };

        // We pass an expression to the query.Select, 
        // so that all of the logic is handled in the EF Core level - 
        // which includes loading the message properties Sender and Recipient
        // and then utilizing them to convert the message into the DTO.
        IQueryable<MessageDto> messagesQuery = query.Select(MessageExtensions.ConvertToDtoProjection());
        
        return await PaginationHelper.CreateAsync(messagesQuery, 
            messageParams.PageNumber, 
            messageParams.PageSize);
    }

    public async Task<IReadOnlyList<MessageDto>> GetMessageThreadAsync(string currentMemberId, string recipientId)
    {
        int res = await context.Messages
            .Where(x => x.RecipientId == currentMemberId
                && x.SenderId == recipientId
                && x.DateRead == null) // Get all the unread messages
            .ExecuteUpdateAsync(setters => setters // Set all of them as read, with the current DateTime
                .SetProperty(m => m.DateRead, DateTime.UtcNow));

        // Get all the messages in the current message thread,
        // and return their DTO's in order by their 'Sent date'
        List<MessageDto> messages = await context.Messages
            .Where(m => (m.RecipientId == currentMemberId && m.SenderId == recipientId)
                || (m.SenderId == currentMemberId && m.RecipientId == recipientId))
            .OrderBy(m => m.MessageSent)
            .Select(MessageExtensions.ConvertToDtoProjection())
            .ToListAsync();

        return messages;
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
