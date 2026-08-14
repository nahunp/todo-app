namespace TodoApp.Application.Common.Interfaces;

/// <summary>
/// Verifies a CAPTCHA challenge token against the provider (Cloudflare
/// Turnstile — see Infrastructure/Captcha/TurnstileCaptchaService.cs).
/// Kept as a thin abstraction, not because another provider is planned,
/// but for the same reason IIdentityService/ITokenService are interfaces:
/// Application shouldn't know or care that this is an HTTP call to a
/// third party, and Application.UnitTests shouldn't need real network
/// access to exercise RegisterCommandHandler.
/// </summary>
public interface ICaptchaService
{
    Task<bool> VerifyAsync(string token, CancellationToken cancellationToken);
}
