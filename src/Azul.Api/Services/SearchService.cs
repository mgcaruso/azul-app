using Azul.Api.Common.Pagination;
using Azul.Api.DTOs;

namespace Azul.Api.Services;

// Buscador principal: junta servicios y proveedores usando las búsquedas de cada service.
public class SearchService(IOfferingService offeringService, IProviderService providerService) : ISearchService
{
    private const int OfferingsLimit = 12;
    private const int ProvidersLimit = 6;

    public async Task<SearchResultDto> SearchAsync(SearchQuery query)
    {
        var text = query.Text.Trim();

        // Una consulta después de la otra: el DbContext no admite dos a la vez.
        var offerings = await offeringService.SearchAsync(
            new OfferingSearchQuery { Text = text, PageSize = OfferingsLimit });
        var providers = await providerService.SearchByNameOrServiceNameAsync(
            text, new PageQuery { PageSize = ProvidersLimit });
        return new SearchResultDto { Offerings = offerings, Providers = providers };
    }
}
