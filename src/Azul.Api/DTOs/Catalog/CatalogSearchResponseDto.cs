using Azul.Api.DTOs.Offerings;
using Azul.Api.DTOs.Providers;

namespace Azul.Api.DTOs.Catalog;

// Respuesta de GET /api/search: la página de proveedores y los servicios cuyo nombre coincide con el texto.
public class CatalogSearchResponseDto
{
    public required ProviderSearchPageDto Providers { get; set; }

    // Vacío si no hay texto.
    public List<OfferingSummaryDto> Offerings { get; set; } = [];
}
