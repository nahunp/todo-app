namespace TodoApp.Application.Common.Interfaces;

/// <summary>
/// Issues access tokens. Just this one method for now - refresh tokens are
/// a deliberate fast-follow, not in this slice (see the auth PR/issue for
/// why: keeps this first cut reviewable, and a short-lived access token
/// without rotation is still a reasonable, if less convenient, starting
/// point).
/// </summary>
public interface ITokenService
{
    /// <summary>Returns the encoded JWT and its absolute expiry.</summary>
    (string AccessToken, DateTimeOffset ExpiresAt) GenerateAccessToken(string userId, string email);
}
