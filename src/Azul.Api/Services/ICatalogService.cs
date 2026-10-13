using Azul.Api.DTOs;

namespace Azul.Api.Services;

public interface ICatalogService
{
    Task<CatalogInitializeDto> GetCatalogAsync();
    Task<SearchResponseDto> GetProvidersCatalogAsync(SearchQuery query);
}
