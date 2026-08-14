using MediatR;
using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

public record LoginResult(string AccessToken, DateTimeOffset ExpiresAt);

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IIdentityService identityService, ITokenService tokenService)
    {
        _identityService = identityService;
        _tokenService = tokenService;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userId = await _identityService.ValidateCredentialsAsync(request.Email, request.Password)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(userId, request.Email);

        return new LoginResult(accessToken, expiresAt);
    }
}
