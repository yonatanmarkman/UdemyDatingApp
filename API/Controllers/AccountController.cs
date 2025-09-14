using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(AppDbContext context, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")] // api/account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await EmailExists(registerDto.Email))
        {
            return BadRequest("Email unavailable. ");
        }

        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            Email = registerDto.Email,
            DisplayName = registerDto.DisplayName,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user.ConvertToUserDto(tokenService);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        AppUser? user = await context.Users.SingleOrDefaultAsync(user => user.Email == loginDto.Email);

        if (user == null)
            return Unauthorized("Invalid login. ");

        using var hmac = new HMACSHA512(user.PasswordSalt);

        byte[] passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < passwordHash.Length; i++)
        {
            if (user.PasswordHash[i] != passwordHash[i])
                return Unauthorized("Invalid login. ");
        }

        return user.ConvertToUserDto(tokenService);
    }

    private async Task<bool> EmailExists(string email)
    {
        return await context.Users.AnyAsync(user =>
                user.Email.ToLower() == email.ToLower());
    }
}
