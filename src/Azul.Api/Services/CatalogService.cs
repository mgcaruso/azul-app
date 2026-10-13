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

    // GET /api/search. La lógica está en ProviderService, que tiene el DbContext y la proyección de la card.
    public async Task<SearchResponseDto> GetProvidersCatalogAsync(SearchQuery query)
    {
        return await providerService.GetProvidersCatalogAsync(query);
    }
}
