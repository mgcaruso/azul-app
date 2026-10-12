using Azul.Api.DTOs;

namespace Azul.Api.Services;

public interface ICatalogService
{
    Task<CatalogInitializeDto> GetCatalogAsync();
    Task<SearchResultDto> SearchAsync(SearchQuery query);
}
