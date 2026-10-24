namespace Azul.Api.Services.Auth;

public interface IGoogleTokenValidator
{
    Task<GoogleUser?> ValidateAsync(string idToken);
}