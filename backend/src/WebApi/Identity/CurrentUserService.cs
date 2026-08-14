using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TodoApp.Application.Common.Interfaces;

namespace TodoApp.WebApi.Identity;

/// <summary>
/// Application's ICurrentUserService, implemented here (not Infrastructure)
/// because it's genuinely a web-hosting concern — it reads the validated
/// JWT's claims off the current HttpContext, which only exists because
/// there's an HTTP request happening at all.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Relies on Program.cs setting MapInboundClaims = false on the JWT
    // Bearer options — without that, ASP.NET Core silently rewrites short
    // claim names like "sub" into long ClaimTypes URIs when building the
    // ClaimsPrincipal, and this lookup would quietly return null instead
    // of the user id. Documented here so the two don't drift apart.
    public string? UserId => _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
}
