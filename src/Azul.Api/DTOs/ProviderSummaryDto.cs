using Azul.Api.Entities;

namespace Azul.Api.DTOs;

// Lo que muestra la card: foto, nombre, rubro, dos servicios, descripción y botones de contacto.
public class ProviderSummaryDto
{
    public int Id { get; set; }
    public required string DisplayName { get; set; }
    public ProviderType Type { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Description { get; set; }
    public required string PhoneNumber { get; set; }
    public string? InstagramUsername { get; set; }

    // Rubro: la categoría de su primer servicio por orden alfabético.
    public string? CategoryName { get; set; }

    // Los dos primeros servicios; el resto se cuenta con OfferingCount ("y N más").
    public List<OfferingSummaryDto> TopOfferings { get; set; } = [];
    public int OfferingCount { get; set; }
}
