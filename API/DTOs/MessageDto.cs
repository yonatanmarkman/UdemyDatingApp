using System;

namespace API.DTOs;

public class MessageDto
{
    // Message Id
    public required string Id { get; set; }

    // Sender info fields
    public required string SenderId { get; set; }
    public required string SenderDisplayName { get; set; }
    public string? SenderImageUrl { get; set; }

    // Recipient info fields 
    public required string RecipientId { get; set; }
    public required string RecipientDisplayName { get; set; }
    public string? RecipientImageUrl { get; set; }

    // Actual message data
    public required string Content { get; set; }
    public DateTime? DateRead { get; set; }
    public DateTime MessageSent { get; set; }
}
