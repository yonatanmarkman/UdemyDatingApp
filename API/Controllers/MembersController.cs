using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MembersController(IMemberRepository memberRepository) : BaseApiController
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
}
