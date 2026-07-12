using System;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IMessageRepository
{
    bool AddMessage(Message message);
    bool DeleteMessage(Message message);
    Task<Message?> GetMessageAsync(string messageId);
    Task<PaginatedResult<MessageDto>> GetMessagesForMemberAsync(
        MessageParams messageParams);
    Task<IReadOnlyList<MessageDto>> GetMessageThreadAsync(
        string currentMemberId, 
        string recipientId);
    Task<MessageDto?> GetLastMessageInThreadAsync(
        string currentMemberId, 
        string recipientId);
    Task<bool> SaveAllAsync();
}