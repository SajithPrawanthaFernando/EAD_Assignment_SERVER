// JwtIssuer.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace App.Auth;

public interface IJwtIssuer
{
    string Issue(string userId, string email, IEnumerable<Role> roles);
}

public sealed class JwtIssuer : IJwtIssuer
{
    private readonly IConfiguration _cfg;
    public JwtIssuer(IConfiguration cfg) => _cfg = cfg;

    public string Issue(string userId, string email, IEnumerable<Role> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Name, email)
        };
        foreach (var r in roles)
            claims.Add(new Claim(ClaimTypes.Role, r.ToString()));

        var token = new JwtSecurityToken(
            issuer: _cfg["Jwt:Issuer"],
            audience: _cfg["Jwt:Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(6),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
