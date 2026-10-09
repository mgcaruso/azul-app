using Azul.Api.Common.Pagination;
using Azul.Api.DTOs;

namespace Azul.Api.Services;

public interface IProviderService
{
    Task<PagedResult<ProviderSummaryDto>> GetAllAsync(ProviderSearchQuery query);
    Task<ProviderDto> CreateAsync(ProviderSaveDto providerSaveDto);
    Task<ProviderDto> GetByIdAsync(int id);
    Task<PagedResult<ProviderSummaryDto>> GetByOfferingAsync(int offeringId, PageQuery pageQuery);
}
