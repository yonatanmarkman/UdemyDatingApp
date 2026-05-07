using System;

namespace API.DTOs;

public class SendMessageDto
{
    public required string RecipientId { get; set; }
    public required string Content { get; set; }
}
