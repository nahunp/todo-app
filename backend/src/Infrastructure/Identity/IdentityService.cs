using Microsoft.AspNetCore.Identity;
using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(bool Succeeded, string? UserId, IReadOnlyList<string> Errors)> CreateUserAsync(string email, string password)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
        };

        var result = await _userManager.CreateAsync(user, password);

        return result.Succeeded
            ? (true, user.Id, Array.Empty<string>())
            : (false, null, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<string?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return null;

        // CheckPasswordAsync, not SignInManager.PasswordSignInAsync — this
        // is a stateless API (JWT bearer tokens, no cookie), there's no
        // ASP.NET Core "signed in" session to establish.
        var passwordValid = await _userManager.CheckPasswordAsync(user, password);

        return passwordValid ? user.Id : null;
    }
}
