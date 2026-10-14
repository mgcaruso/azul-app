using Azul.Api.DTOs.Catalog;
using Azul.Api.DTOs.Search;

namespace Azul.Api.Services.Catalog;

public interface ICatalogService
{
    Task<CatalogInitializeDto> GetCatalogAsync();
    Task<SearchResponseDto> GetProvidersCatalogAsync(SearchQuery query);
}
