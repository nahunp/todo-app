using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Application.UnitTests.Common;

/// <summary>
/// Passes any non-empty token by default (real network access to Cloudflare
/// has no place in a unit test) — construct with alwaysPasses: false for
/// the one test that needs to exercise RegisterCommandHandler's rejection
/// path.
/// </summary>
public class FakeCaptchaService : ICaptchaService
{
    private readonly bool _alwaysPasses;

    public FakeCaptchaService(bool alwaysPasses = true)
    {
        _alwaysPasses = alwaysPasses;
    }

    public Task<bool> VerifyAsync(string token, CancellationToken cancellationToken) =>
        Task.FromResult(_alwaysPasses);
}
