using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MessagesController(
    IMessageRepository messageRepository,
    IMemberRepository memberRepository
    ) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> SendMessage(
        SendMessageDto sendMessageDto)
    {
        Member? sender = await memberRepository
            .GetMemberByIdAsync(User.GetMemberId());
        Member? recipient = await memberRepository
            .GetMemberByIdAsync(sendMessageDto.RecipientId);

        if (recipient == null 
            || sender == null 
            || sender.Id == sendMessageDto.RecipientId)
        {
            return BadRequest("Cannot send this message");
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
            return message.ConvertToDto();
        }

        return BadRequest("Failed to send message. ");
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<MessageDto>>> GetMessagesByContainer(
        [FromQuery] MessageParams messageParams)
    {
        messageParams.MemberId = User.GetMemberId();

        PaginatedResult<MessageDto> result
             = await messageRepository.GetMessagesForMemberAsync(messageParams);

        return result;
    }

    [HttpGet("thread/{recipientId}")]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>>  GetMessageThread(string recipientId)
    {
        return Ok(await messageRepository.GetMessageThreadAsync(User.GetMemberId(), recipientId));
    }
}
