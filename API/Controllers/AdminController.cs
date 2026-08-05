using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController(UserManager<AppUser> userManager) : BaseApiController
{
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        List<AppUser> users = await userManager.Users.ToListAsync();
        var userRolesList = new List<RoleDto>();

        foreach (AppUser user in users)
        {
            IList<string> roles = await userManager.GetRolesAsync(user);
            userRolesList.Add(new RoleDto()
            {
                Id = user.Id,
                Email = user.Email,
                Roles = [.. roles]
            });
        }

        return Ok(userRolesList);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{userId}")]
    public async Task<ActionResult> EditRoles(string userId, [FromQuery] string roles)
    {
        if (string.IsNullOrEmpty(roles))
            return BadRequest("At least one role must be selected. ");

        string[] selectedRoles = roles.Split(",").ToArray();

        AppUser? user = await userManager.FindByIdAsync(userId);

        if (user == null)
            return BadRequest($"User {userId} not found. ");

        IList<string> existingUserRoles
             = await userManager.GetRolesAsync(user);

        // Remove the existing roles from the user,
        // and then add the selected roles.
        IdentityResult result = await userManager.RemoveFromRolesAsync(user, existingUserRoles);
        if (!result.Succeeded)
            return BadRequest("Failed to remove from roles. ");

        result = await userManager.AddToRolesAsync(user, selectedRoles);
        if (!result.Succeeded)
            return BadRequest("Failed to add to roles. ");

        IList<string> rolesList = await userManager.GetRolesAsync(user);

        return Ok(rolesList);
    }
    
    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public async Task<ActionResult> GetPhotosForModeration()
    {
        return Ok("Only admins and moderators can see this");
    }
}
