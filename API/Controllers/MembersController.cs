using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MembersController(
    IMemberRepository memberRepository,
    IPhotoService photoService)
     : BaseApiController
{
    [HttpGet] // /api/members
    public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers()
    {
        IReadOnlyList<Member> result = await memberRepository.GetMembersAsync();

        return Ok(result);
    }

    [HttpGet("{id}")] // /api/members/bob-id
    public async Task<ActionResult<Member>> GetMember(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest("Member ID cannot be null or empty");

        try
        {
            Member? member = await memberRepository.GetMemberByIdAsync(id);

            if (member == null)
                return NotFound();

            return member;
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}/photos")]
    public async Task<ActionResult<IReadOnlyList<Photo>>> GetMemberPhotos(string id)
    {
        IReadOnlyList<Photo> result
            = await memberRepository.GetPhotosForMemberAsync(id);

        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateMember(MemberUpdateDto memberUpdateDto)
    {
        string? memberId = User.GetMemberId();

        Member? member = await memberRepository.GetMemberForUpdateAsync(memberId);

        if (member == null)
            return BadRequest("Could not get member. ");

        // Update the member fields
        member.DisplayName = memberUpdateDto.DisplayName ?? member.DisplayName;
        member.Description = memberUpdateDto.Description ?? member.Description;
        member.City = memberUpdateDto.City ?? member.City;
        member.Country = memberUpdateDto.Country ?? member.Country;

        // Update the User entity
        member.User.DisplayName = memberUpdateDto.DisplayName ?? member.User.DisplayName;

        if (await memberRepository.SaveAllAsync())
            return NoContent();

        return BadRequest("Failed to update member. ");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<Photo>> AddPhoto([FromForm] IFormFile file)
    {
        Member? member = await memberRepository.GetMemberForUpdateAsync(User.GetMemberId());

        if (member == null)
            return BadRequest("Cannot update member. ");

        ImageUploadResult result = await photoService.UploadPhotoAsync(file);

        if (result.Error != null)
            return BadRequest(result.Error.Message);
        
        // Result is success - create photo object, and update the member object
        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,
            MemberId = User.GetMemberId()
        };

        // Update the profile picture URL
        if (member.ImageUrl == null)
        {
            member.ImageUrl = photo.Url;
            member.User.ImageUrl = photo.Url;
        }

        // Add the actual photo object reference into the member object
        member.Photos.Add(photo);

        if (await memberRepository.SaveAllAsync())
            return photo;
        
        // Saving failed - bad request
        return BadRequest("Problem adding photo. ");
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        Member? member = await memberRepository
            .GetMemberForUpdateAsync(User.GetMemberId());
        
        if (member == null)
            return BadRequest("Cannot get member from token");

        Photo? photo = member.Photos
            .FirstOrDefault(x => x.Id == photoId);

        if (photo == null || member.ImageUrl == photo.Url)
        {
            return BadRequest("Cannot set this as main image");
        }

        member.ImageUrl = photo.Url;
        member.User.ImageUrl = photo.Url;

        if (await memberRepository.SaveAllAsync())
            return NoContent();
        
        return BadRequest("Problem setting main photo. ");
    }
}
