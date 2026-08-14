using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Infrastructure.Captcha;

/// <summary>
/// Calls Cloudflare Turnstile's siteverify endpoint. The secret key lives
/// in configuration (Captcha:SecretKey — User Secrets locally, an Azure App
/// Setting in production, same pattern as Jwt:SigningKey), never in source.
///
/// Local dev default (see appsettings.json) is Cloudflare's own published
/// always-passes test secret (1x0000...AA) paired with the matching test
/// site key on the frontend — safe to commit, it's not a real secret, it's
/// documented by Cloudflare specifically so integrations can be built and
/// tested without a real Turnstile site. Production uses the real secret
/// from the Turnstile site registered for the actual deployed domain.
/// </summary>
public class TurnstileCaptchaService : ICaptchaService
{
    private const string VerifyUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

    private readonly HttpClient _httpClient;
    private readonly string _secretKey;
    private readonly ILogger<TurnstileCaptchaService> _logger;

    public TurnstileCaptchaService(HttpClient httpClient, IConfiguration configuration, ILogger<TurnstileCaptchaService> logger)
    {
        _httpClient = httpClient;
        _secretKey = configuration["Captcha:SecretKey"]
            ?? throw new InvalidOperationException(
                "Captcha:SecretKey is not configured. Set it in User Secrets (same secrets.json as the connection string) for local dev, or as an App Setting in production.");
        _logger = logger;
    }

    public async Task<bool> VerifyAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            var response = await _httpClient.PostAsync(
                VerifyUrl,
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"] = _secretKey,
                    ["response"] = token,
                }),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<TurnstileVerifyResponse>(cancellationToken: cancellationToken);
            return result?.Success ?? false;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // Cloudflare being unreachable shouldn't look like a 500 to the
            // caller — it's the same "couldn't confirm you're not a bot"
            // outcome as an actually-failed verification, not a server bug.
            // Logged so a real outage is still visible in Azure's log
            // stream, just not surfaced as an unhandled exception.
            _logger.LogWarning(ex, "Turnstile verification request failed");
            return false;
        }
    }

    private record TurnstileVerifyResponse([property: JsonPropertyName("success")] bool Success);
}
