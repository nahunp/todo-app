using Microsoft.AspNetCore.Identity;

namespace TodoApp.Infrastructure.Identity;

/// <summary>
/// Identity's user type. Deliberately empty for now — no custom profile
/// fields yet. Domain/Application never reference this type directly (see
/// TodoList.OwnerId, a plain string) — this class existing at all is an
/// Infrastructure concern.
/// </summary>
public class ApplicationUser : IdentityUser
{
}
