using Azul.Api.DTOs;

namespace Azul.Api.Services;

public interface ISearchService
{
    Task<SearchResultDto> SearchAsync(SearchQuery query);
}
