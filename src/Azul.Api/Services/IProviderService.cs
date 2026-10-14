using Azul.Api.Common.Pagination;
using Azul.Api.DTOs.Providers;
using Azul.Api.DTOs.Search;

namespace Azul.Api.Services;

public interface IProviderService
{
    Task<PagedResult<ProviderSummaryDto>> GetAllAsync(ProviderSearchQuery query);
    Task<ProviderDto> CreateAsync(ProviderSaveDto providerSaveDto);
    Task<ProviderDto> GetByIdAsync(int id);
    Task UpdateAsync(int id, ProviderSaveDto providerSaveDto);
    Task DeleteAsync(int id);
    Task<PagedResult<ProviderSummaryDto>> GetByOfferingAsync(int offeringId, PageQuery pageQuery);
    Task<List<ProviderSummaryDto>> GetLatestAsync();
    
    Task<List<ProviderSummaryDto>> GetFeaturedAsync();

    Task<SearchResponseDto> GetProvidersCatalogAsync(SearchQuery query);
}
