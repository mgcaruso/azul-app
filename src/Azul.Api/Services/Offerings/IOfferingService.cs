using Azul.Api.Common.Pagination;
using Azul.Api.DTOs.Offerings;

namespace Azul.Api.Services.Offerings;

public interface IOfferingService
{
    Task<PagedResult<OfferingListItemDto>> SearchAsync(OfferingSearchQuery query);
    Task<OfferingDto> GetByIdAsync(int id);
    Task<List<OfferingSummaryDto>> GetByCategoryAsync(int categoryId);
    Task<List<OfferingSummaryDto>> GetRandomAsync(int count);
    Task<OfferingDto> CreateAsync(OfferingSaveDto offeringSaveDto);
    Task UpdateAsync(int id, OfferingSaveDto offeringSaveDto);
    Task DeleteAsync(int id);
}
