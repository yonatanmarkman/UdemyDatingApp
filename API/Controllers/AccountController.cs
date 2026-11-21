using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")] // api/account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        var user = new AppUser
        {
            DisplayName = registerDto.DisplayName,
            Email = registerDto.Email,
            UserName = registerDto.Email,
            Member = new Member
            {
                DisplayName = registerDto.DisplayName,
                Gender = registerDto.Gender,
                City = registerDto.City,
                Country = registerDto.Country,
                DateOfBirth = registerDto.DateOfBirth
            }
        };

        IdentityResult result = await userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("identity", error.Description);
            }

            return ValidationProblem();
        }

        await userManager.AddToRoleAsync(user, "Member");

        await SetRefreshTokenCookie(user);

        return await user.ConvertToUserDto(tokenService);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        AppUser? user = await userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
            return Unauthorized("Invalid login. ");

        // Utilize the Identity method to compare between the user's actual password,
        // and the password that the user is currently attempting to login with
        bool result = await userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!result)
            return Unauthorized("Invalid login. ");
        
        await SetRefreshTokenCookie(user);

        return await user.ConvertToUserDto(tokenService);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<UserDto>> RefreshToken()
    {
        string? refreshToken = Request.Cookies["refreshToken"];
        if (refreshToken == null)
            return NoContent();

        AppUser? user = await userManager.Users
            .FirstOrDefaultAsync(user => user.RefreshToken == refreshToken 
                            && user.RefreshTokenExpiry > DateTime.UtcNow);
        
        if (user == null)
            return Unauthorized();
        
        await SetRefreshTokenCookie(user);

        return await user.ConvertToUserDto(tokenService);
    }

    private async Task SetRefreshTokenCookie(AppUser user)
    {
        string refreshToken = tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        await userManager.UpdateAsync(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        // When the user logs out, we immediately update the RefreshToken data,
        // so that he isn't "logged in" when he enters the page again, after logout.
        await userManager.Users
            .Where(user => user.Id == User.GetMemberId())
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(appUser => appUser.RefreshToken, _ => null) // Set the RefreshToken to null
                .SetProperty(appUser => appUser.RefreshTokenExpiry, _ => null) // Set the RefreshToken to null
                );
        
        // Delete the 'refreshToken' field from the cookies as well.
        Response.Cookies.Delete("refreshToken");

        return Ok();
    }
}
