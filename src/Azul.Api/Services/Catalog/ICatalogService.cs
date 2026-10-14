using Azul.Api.DTOs.Catalog;

namespace Azul.Api.Services.Catalog;

public interface ICatalogService
{
    Task<CatalogInitializeDto> GetCatalogAsync();
    Task<CatalogSearchResponseDto> CatalogSearchAsync(CatalogSearchQuery query);
}
