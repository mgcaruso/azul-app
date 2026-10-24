using Azul.Api.Services.Auth;

namespace Azul.Api.Tests.Auth;

// Reemplaza a Google en los tests. "google:<sub>:<nombre>" es un token válido; cualquier otro, inválido.
public class FakeGoogleTokenValidator : IGoogleTokenValidator
{
    public Task<GoogleUser?> ValidateAsync(string idToken)
    {
        var parts = idToken.Split(':');
        if (parts is not ["google", var subject, var name])
        {
            return Task.FromResult<GoogleUser?>(null);
        }

        return Task.FromResult<GoogleUser?>(
            new GoogleUser(subject, $"{subject}@test.com", name, $"https://example.com/{subject}.jpg"));
    }

    public static string TokenFor(string subject, string name = "Ana Test") => $"google:{subject}:{name}";
}
