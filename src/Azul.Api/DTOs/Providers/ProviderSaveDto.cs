using Azul.Api.Entities;

namespace Azul.Api.DTOs.Providers;

public class ProviderSaveDto
{
    public ProviderType? Type { get; set; }

    // Individual: nombre y apellido. El service arma DisplayName.
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    // Emprendimiento y Negocio: la marca.
    public string? DisplayName { get; set; }

    public string? Description { get; set; }
    // TODO: an owner can send any PhotoKey and the profile would show that path. Before opening to the public, stop accepting it
    // from the owner's PUT/POST and let only the photo upload flow write it.
    public string? PhotoKey { get; set; }
    // Hace falta al menos uno de los dos teléfonos. Formato internacional: +54 9 2281 40-1234 (WhatsApp), +54 2281 45-6789 (fijo).
    public string? PhoneNumber { get; set; }
    public string? CallPhone { get; set; }
    public string? ExtraUrl { get; set; }
    public string? InstagramUsername { get; set; }
    public string? Address { get; set; }
    public string? Hours { get; set; }

    public List<int> OfferingIds { get; set; } = [];

    // La casilla del formulario; con esto el service crea el ProviderConsent.
    public bool AcceptedTerms { get; set; }
}
