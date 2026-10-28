using Azul.Api.Common.Pagination;
using Azul.Api.DTOs.Catalog;
using Azul.Api.DTOs.Providers;

namespace Azul.Api.Services.Providers;

public interface IProviderReadService
{
    Task<PagedResult<ProviderSummaryDto>> GetAllAsync(ProviderSearchQuery query);
    Task<ProviderDto> GetByIdAsync(int id);
    Task<MyProviderDto> GetMineAsync(Guid userId);
    Task<AdminProviderDto> GetAdminByIdAsync(int id);
    Task<List<DuplicateGroupDto>> GetDuplicateGroupsAsync();
    Task<PagedResult<AdminProviderListItemDto>> GetAdminListAsync(AdminProviderSearchQuery query);
    Task<PagedResult<ProviderSummaryDto>> GetByOfferingAsync(int offeringId, PageQuery pageQuery);
    Task<List<ProviderSummaryDto>> GetLatestAsync();
    Task<List<ProviderSummaryDto>> GetFeaturedAsync();
    Task<ProviderSearchPageDto> CatalogSearchAsync(CatalogSearchQuery query);
}
