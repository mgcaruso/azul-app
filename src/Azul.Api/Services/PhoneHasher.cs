using System.Security.Cryptography;
using System.Text;

namespace Azul.Api.Services;

// HMAC-SHA256 del celular normalizado con la clave secreta de la app (ProviderConsent:PhoneHashKey).
// Devuelve 64 caracteres hexadecimales en minúsculas.
public class PhoneHasher(IConfiguration configuration) : IPhoneHasher
{
    private readonly byte[] key = Encoding.UTF8.GetBytes(
        configuration["ProviderConsent:PhoneHashKey"]
        ?? throw new InvalidOperationException("Falta la configuración ProviderConsent:PhoneHashKey."));

    public string Hash(string phoneNumber)
    {
        return Convert.ToHexStringLower(HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(phoneNumber)));
    }
}
