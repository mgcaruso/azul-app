using System.ComponentModel.DataAnnotations;

namespace Azul.Api.Entities;

// Datos de contacto de una ficha. Hace falta al menos uno de los dos teléfonos (WhatsApp o fijo).
public class ProviderContact
{
    // Clave y clave foránea a la vez: es una fila por ficha.
    public int ProviderId { get; set; }
    public Provider Provider { get; set; } = null!;

    // WhatsApp en formato internacional. Argentina: +549XXXXXXXXXX. Puede ser de otro país. Puede repetirse entre fichas.
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    // Teléfono fijo en formato internacional. Argentina: +54, código de área y número. Puede repetirse.
    [MaxLength(20)]
    public string? CallPhone { get; set; }

    // Sin @ y en minúsculas. Único.
    [MaxLength(30)]
    public string? InstagramUsername { get; set; }

    // Enlace extra opcional (menú, turnos, sitio).
    [MaxLength(200)]
    public string? ExtraUrl { get; set; }
}
