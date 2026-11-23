using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MessagesController(IUnitOfWork unitOfWork
    ) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> SendMessage(
        SendMessageDto sendMessageDto)
    {
        Member? sender = await unitOfWork.MemberRepository
            .GetMemberByIdAsync(User.GetMemberId());
        Member? recipient = await unitOfWork.MemberRepository
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

        unitOfWork.MessageRepository.AddMessage(message);

        if (await unitOfWork.Complete())
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
             = await unitOfWork.MessageRepository.GetMessagesForMemberAsync(messageParams);

        return result;
    }

    [HttpGet("thread/{recipientId}")]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetMessageThread(string recipientId)
    {
        IReadOnlyList<MessageDto>? result
             = await unitOfWork.MessageRepository.GetMessageThreadAsync(User.GetMemberId(), recipientId);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(string id)
    {
        string memberId = User.GetMemberId();

        Message? message = await unitOfWork.MessageRepository.GetMessageAsync(id);

        if (message == null 
         || message.SenderId != memberId 
         && message.RecipientId != memberId)
        {
            return BadRequest("Cannot delete this message");
        }

        if (message.SenderId == memberId)
            message.SenderDeleted = true;
        
        if (message.RecipientId == memberId)
            message.RecipientDeleted = true;
        
        // Delete the message from the database only if both sides have deleted it.
        if (message is {SenderDeleted: true, RecipientDeleted: true})
            unitOfWork.MessageRepository.DeleteMessage(message);
        
        if (await unitOfWork.Complete())
            return Ok();
        
        return BadRequest("Could not delete message");
    }
}
