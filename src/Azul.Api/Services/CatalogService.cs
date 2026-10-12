using Azul.Api.Common.Pagination;
using Azul.Api.DTOs;

namespace Azul.Api.Services;

// Arma respuestas que juntan varias entidades usando los otros services (no repite consultas).
public class CatalogService(IOfferingService offeringService, IProviderService providerService) : ICatalogService
{
    private const int OfferingsLimit = 12;
    private const int ProvidersLimit = 6;
    private const int RandomOfferingsLimit = 8;

    // GET /api/catalog/initialize: últimos proveedores + servicios.
    // TODO: cachear esta respuesta (cambia poco y la pide cada visita a la Home). 
    public async Task<CatalogInitializeDto> GetCatalogAsync()
    {
        // Una consulta después de la otra: el DbContext no admite dos a la vez.
        var latestProviders = await providerService.GetLatestAsync();
        var offerings = await offeringService.GetRandomAsync(RandomOfferingsLimit);
        return new CatalogInitializeDto { LatestProviders = latestProviders, Offerings = offerings };
    }

    // Buscador principal: junta servicios y proveedores usando las búsquedas de cada service.
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
