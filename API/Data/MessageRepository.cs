using System;
using API.DTOs;
using API.Entities;
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

    public Task<PaginatedResult<MessageDto>> GetMessagesForMemberAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MessageDto>> GetMessageThreadAsync(string currentMemberId, string recipientId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
