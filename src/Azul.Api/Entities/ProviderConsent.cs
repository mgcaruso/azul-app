using System.ComponentModel.DataAnnotations;

namespace Azul.Api.Entities;

public class ProviderConsent
{
    public int Id { get; set; }

    // Queda en NULL si se borra el proveedor: el consentimiento se conserva como registro.
    public int? ProviderId { get; set; }
    public Provider? Provider { get; set; }

    // HMAC-SHA256 del celular normalizado, en hexadecimal. Nunca el número en claro.
    [MaxLength(64)]
    public required string PhoneHash { get; set; }

    public DateTime AcceptedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
