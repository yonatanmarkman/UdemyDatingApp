using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService(IConfiguration config) : ITokenService
{
    static readonly int SIXTY_FOUR = 64;
    static readonly int SEVEN_DAYS = 7;

    public string CreateToken(AppUser user)
    {
        string tokenKey = config["TokenKey"] ?? throw new Exception("Cannot get token key. ");

        if (tokenKey.Length < SIXTY_FOUR)
        {
            throw new Exception("Token key length must be >= " + SIXTY_FOUR + "characters. ");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id)
        };

        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(SEVEN_DAYS),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

}
