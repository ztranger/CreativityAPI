using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Application.Auth;
using API.Domain.Users;
using Microsoft.IdentityModel.Tokens;

namespace API.Infrastructure.Auth;

public sealed class JwtTokenService : IUserTokenService
{
    private readonly JwtOptions _options;
    private readonly byte[] _keyBytes;

    public JwtTokenService(JwtOptions options)
    {
        _options = options;
        _keyBytes = System.Text.Encoding.UTF8.GetBytes(_options.Key);
    }

    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.PhoneNumber, user.Phone),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(_keyBytes);
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
