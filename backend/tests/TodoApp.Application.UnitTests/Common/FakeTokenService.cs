using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Application.UnitTests.Common;

public class FakeTokenService : ITokenService
{
    public (string AccessToken, DateTimeOffset ExpiresAt) GenerateAccessToken(string userId, string email)
    {
        // Not a real JWT - just enough for a handler test to assert
        // "a token came back and it's tied to the right user," without
        // dragging a real signing key into Application's test project.
        return ($"fake-token-for-{userId}", DateTimeOffset.UtcNow.AddMinutes(60));
    }
}
