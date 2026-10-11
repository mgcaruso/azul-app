using Azul.Api.Entities;

namespace Azul.Api.DTOs;

public class ProviderSaveDto
{
    public ProviderType? Type { get; set; }

    // Individual: nombre y apellido. El service arma DisplayName.
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    // Emprendimiento y Negocio: la marca.
    public string? DisplayName { get; set; }

    public string? Description { get; set; }
    public string? PhotoKey { get; set; }
    public string? PhoneNumber { get; set; }
    public string? InstagramUsername { get; set; }
    public string? Address { get; set; }
    public string? Hours { get; set; }

    public List<int> OfferingIds { get; set; } = [];

    // La casilla del formulario; con esto el service crea el ProviderConsent.
    public bool AcceptedTerms { get; set; }
}
