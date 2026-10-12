using Azul.Api.DTOs;

namespace Azul.Api.Services;

public interface ICatalogService
{
    Task<CatalogInitializeDto> GetCatalogAsync();
    Task<SearchResultDto> SearchAsync2(SearchQuery query);
    Task<SearchResponseDto> SearchAsync(SearchQuery query);
}
