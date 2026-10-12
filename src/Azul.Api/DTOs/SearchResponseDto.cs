namespace Azul.Api.DTOs;

// Respuesta de GET /api/search: la página de proveedores y los filtros que se pueden sumar.
// Reemplaza a SearchResultDto cuando se borre SearchAsync2.
public class SearchResponseDto
{
    public required ProviderSearchPageDto Providers { get; set; }
    public required SearchFiltersDto Filters { get; set; }
}
