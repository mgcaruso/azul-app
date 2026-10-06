using Azul.Api.DTOs;

namespace Azul.Api.Services;

public interface IOfferingService
{
    Task<OfferingSearchResultDto> SearchAsync(OfferingSearchQuery query);
    Task<OfferingDto> GetByIdAsync(int id);
    Task<List<OfferingSummaryDto>> GetByCategoryAsync(int categoryId);
    Task<OfferingDto> CreateAsync(OfferingSaveDto offeringSaveDto);
    Task UpdateAsync(int id, OfferingSaveDto offeringSaveDto);
    Task DeleteAsync(int id);
}
