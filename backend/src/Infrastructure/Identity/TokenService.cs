using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Infrastructure.Identity;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string AccessToken, DateTimeOffset ExpiresAt) GenerateAccessToken(string userId, string email)
    {
        // Signing key lives in User Secrets ("Jwt:SigningKey"), same secrets.json
        // as the connection string - never hardcoded, never committed. Issuer/
        // Audience aren't secret, just identifying strings; fine in appsettings.json.
        var signingKeyValue = _configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException(
                "Jwt:SigningKey is not configured. Set it in User Secrets (same secrets.json as the connection string).");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKeyValue));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            // A fresh id per token, not derived from anything else - lets a
            // future revocation list key on individual tokens if that's ever
            // needed, without changing the token shape.
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        // 60 minutes: no refresh token in this slice (see ITokenService's
        // doc comment) - long enough to not be annoying during a dev
        // session, short enough that a leaked token doesn't stay valid for
        // long. Revisit once refresh tokens exist.
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(60);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
