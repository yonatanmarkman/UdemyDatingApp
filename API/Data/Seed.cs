using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.DTOs;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        List<SeedUserDto>? members = await LoadSeedData();
        if (members == null) return;

        foreach (SeedUserDto member in members)
        {
            using var hmac = new HMACSHA512();

            AppUser user = CreateAppUser(member, hmac);
            context.Users.Add(user);
        }

        await context.SaveChangesAsync();
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

    private static AppUser CreateAppUser(SeedUserDto member, HMACSHA512 hmac)
    {
        var user = new AppUser
        {
            Id = member.Id,
            Email = member.Email,
            DisplayName = member.DisplayName,
            ImageUrl = member.ImageUrl,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd")),
            PasswordSalt = hmac.Key,
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
