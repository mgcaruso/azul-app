using System.ComponentModel.DataAnnotations;

namespace Azul.Api.Entities;

public class Provider
{
    public int Id { get; set; }

    public ProviderType Type { get; set; }

    // Individual: "Nombre Apellido", lo arma el service. Emprendimiento y Negocio: la marca.
    [MaxLength(100)]
    public required string DisplayName { get; set; }

    // Solo Individual.
    [MaxLength(50)]
    public string? FirstName { get; set; }

    [MaxLength(50)]
    public string? LastName { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? PhotoKey { get; set; }

    // Siempre WhatsApp, normalizado a +549XXXXXXXXXX. Puede repetirse.
    [MaxLength(20)]
    public required string PhoneNumber { get; set; }

    // Sin @ y en minúsculas. Único.
    [MaxLength(30)]
    public string? InstagramUsername { get; set; }

    // Obligatoria para Business.
    [MaxLength(150)]
    public string? Address { get; set; }

    [MaxLength(200)]
    public string? Hours { get; set; }

    public ProviderStatus Status { get; set; } = ProviderStatus.Pending;

    public DateTime? FirstPublishedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<ProviderOffering> Offerings { get; set; } = [];

    public List<ProviderConsent> Consents { get; set; } = [];
}
