using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using mylibrary.Models;

namespace mylibrary.Helpers;

public class JwtTokenHelper
{
    private readonly JwtSettings _jwtSettings;
    public JwtTokenHelper(IOptions<JwtSettings> jwtSettings)
	{
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(string username)
    {
        var claims = new List<Claim>
        {
            new Claim("UserName", username),
            new Claim("RelationId", Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

