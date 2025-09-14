using System;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class MembersController(AppDbContext context) : BaseApiController
{
    [HttpGet] // /api/members
    public async Task<ActionResult<IEnumerable<AppUser>>> GetMembers()
    {
        List<AppUser> members = await context.Users.ToListAsync();

        return members;
    }

    [Authorize]
    [HttpGet("{id}")] // /api/members/bob-id
    public async Task<ActionResult<AppUser>> GetMember(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest("Member ID cannot be null or empty");

        try
        {
            AppUser? member = await context.Users.FindAsync(id);

            if (member == null)
                return NotFound();

            return member;
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
