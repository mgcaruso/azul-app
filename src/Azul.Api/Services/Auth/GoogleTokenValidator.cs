using Google.Apis.Auth;

namespace Azul.Api.Services.Auth;

public class GoogleTokenValidator(IConfiguration configuration, ILogger<GoogleTokenValidator> logger)
    : IGoogleTokenValidator
{
    private readonly string _clientId = configuration["Google:ClientId"]
        ?? throw new InvalidOperationException("Falta la configuración Google:ClientId.");

    public async Task<GoogleUser?> ValidateAsync(string idToken)
    {
        // los settings son instrucciones para la validacion. si no comple por ej co nl audience, sale por el invalidjwt exception
        var settings = new GoogleJsonWebSignature.ValidationSettings { Audience = [_clientId] };
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            if (!payload.EmailVerified)
            {
                return null;
            }

            return new GoogleUser(payload.Subject, payload.Email, payload.Name ?? payload.Email, payload.Picture);
        }
        catch (InvalidJwtException e)
        {
            logger.LogWarning(e, "Token de Google inválido");
            return null;
        }
    }
}
