using Azul.Api.Common.Pagination;

namespace Azul.Api.DTOs;

// Resultado del buscador principal: la primera página de servicios y la de proveedores.
// Para ver más de cada uno están /api/offerings?text= y la lista de proveedores de cada servicio.
public class SearchResultDto
{
    public required PagedResult<OfferingSummaryDto> Offerings { get; set; }
    public required PagedResult<ProviderSummaryDto> Providers { get; set; }
}
