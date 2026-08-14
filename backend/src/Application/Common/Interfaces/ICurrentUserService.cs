namespace TodoApp.Application.Common.Interfaces;

/// <summary>
/// Who's making this request, as far as Application handlers are concerned.
/// Deliberately not HttpContext or a claims principal — Application doesn't
/// know what a request even is (could be HTTP, could be a test). WebApi
/// implements this by reading the validated JWT's claims.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Null if there's no authenticated user — endpoints that need one are
    /// protected with .RequireAuthorization(), so in practice this is only
    /// null in contexts that bypass that (tests calling a handler directly
    /// without going through the pipeline).
    /// </summary>
    string? UserId { get; }
}
