namespace Azul.Api.Common.Exceptions;

public class UnauthorizedException : AppException
{
    
    public UnauthorizedException(string code, string message) : base(code, message)
    {
    }

    public static UnauthorizedException InvalidGoogleToken()
        => new("auth.invalid_google_token", "No pudimos verificar tu cuenta de Google.");
    public static UnauthorizedException NotSignedIn()
        => new("auth.not_signed_in", "Tenés que iniciar sesión.");
}