using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(UserManager<AppUser> userManager)
    {
        if (await userManager.Users.AnyAsync()) return;

        List<SeedUserDto>? members = await LoadSeedData();
        if (members == null) return;

        foreach (SeedUserDto member in members)
        {
            AppUser user = CreateAppUser(member);

            IdentityResult creationResult = await userManager.CreateAsync(user, "Pa$$w0rd");
            if (!creationResult.Succeeded)
            {
                Console.WriteLine(creationResult.Errors.First().Description);
            }
            IdentityResult addToRoleResult = await userManager.AddToRoleAsync(user, "Member");
            if (!addToRoleResult.Succeeded)
            {
                Console.WriteLine(addToRoleResult.Errors.First().Description);
            }
        }

        var admin = new AppUser
        {
            UserName = "admin@test.com",
            Email = "admin@test.com",
            DisplayName = "Admin"
        };
        
        await userManager.CreateAsync(admin, "Pa$$w0rd");
        await userManager.AddToRolesAsync(admin, ["Admin", "Moderator"]);
    }

    private static async Task<List<SeedUserDto>?> LoadSeedData()
    {
        string memberData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        List<SeedUserDto>? members
             = JsonSerializer.Deserialize<List<SeedUserDto>>(memberData);

        if (members == null)
        {
            Console.WriteLine("No members in seed data");
        }

        return members;
    }

    private static AppUser CreateAppUser(SeedUserDto member)
    {
        var user = new AppUser
        {
            Id = member.Id,
            Email = member.Email,
            UserName = member.Email,
            DisplayName = member.DisplayName,
            ImageUrl = member.ImageUrl,
            Member = new Member
            {
                Id = member.Id,
                DisplayName = member.DisplayName,
                Description = member.Description,
                DateOfBirth = member.DateOfBirth,
                ImageUrl = member.ImageUrl,
                Gender = member.Gender,
                City = member.City,
                Country = member.Country,
                LastActive = member.LastActive,
                Created = member.Created
            }
        };

        user.Member.Photos.Add(new Photo
        {
            Url = member.ImageUrl!,
            Member = user.Member,
            MemberId = member.Id
        });

        return user;
    }
}
