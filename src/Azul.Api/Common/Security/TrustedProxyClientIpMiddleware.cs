using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Azul.Api.Common.Security;

// La IP real del usuario solo la ve Cloudflare (primer salto). El Worker la manda en X-Client-IP junto con
// X-Proxy-Secret; si el secreto coincide con Proxy:Secret, se usa esa IP como RemoteIpAddress.
// Así el rate limit y los logs ven a cada usuario por separado, sin tocar las políticas.
// Sin secreto válido (o sin Proxy:Secret configurado, como en desarrollo) no se cambia nada: queda la IP
// que dejó UseForwardedHeaders, que para quien entra directo a run.app ya es su IP real.
public class TrustedProxyClientIpMiddleware(RequestDelegate next, IConfiguration configuration)
{
    public const string ClientIpHeader = "X-Client-IP";
    public const string SecretHeader = "X-Proxy-Secret";

    private readonly byte[]? _secret = ReadSecret(configuration);

    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Request.Headers;

        if (_secret is not null
            && IsValidSecret(headers[SecretHeader].ToString(), _secret)
            && IPAddress.TryParse(headers[ClientIpHeader].ToString(), out var clientIp))
        {
            context.Connection.RemoteIpAddress = clientIp;
        }

        // El secreto no sigue de largo: así no puede terminar en un log ni en ningún otro lado.
        headers.Remove(SecretHeader);

        return next(context);
    }

    private static byte[]? ReadSecret(IConfiguration configuration)
    {
        var secret = configuration["Proxy:Secret"];
        return string.IsNullOrWhiteSpace(secret) ? null : Encoding.UTF8.GetBytes(secret);
    }

    // Comparación en tiempo constante: no deja adivinar el secreto midiendo cuánto tarda la respuesta.
    private static bool IsValidSecret(string received, byte[] expected) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(received), expected);
}
