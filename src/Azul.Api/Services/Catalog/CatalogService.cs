using Azul.Api.DTOs.Catalog;
using Azul.Api.DTOs.Search;
using Azul.Api.Services.Offerings;
using Azul.Api.Services.Providers;

namespace Azul.Api.Services.Catalog;

// Arma respuestas que juntan varias entidades usando los otros services (no repite consultas).
public class CatalogService(IOfferingService offeringService, IProviderReadService providerReadService) : ICatalogService
{
    private const int RandomOfferingsLimit = 8;

    // GET /api/catalog/initialize: últimos proveedores + servicios.
    // TODO: cachear esta respuesta (cambia poco y la pide cada visita a la Home). 
    public async Task<CatalogInitializeDto> GetCatalogAsync()
    {
        // Una consulta después de la otra: el DbContext no admite dos a la vez.
        var latestProviders = await providerReadService.GetLatestAsync();
        var offerings = await offeringService.GetRandomAsync(RandomOfferingsLimit);
        return new CatalogInitializeDto { LatestProviders = latestProviders, Offerings = offerings };
    }

    // GET /api/search. La lógica está en ProviderReadService, que tiene el DbContext y la proyección de la card.
    public async Task<SearchResponseDto> GetProvidersCatalogAsync(SearchQuery query)
    {
        return await providerReadService.GetProvidersCatalogAsync(query);
    }
}
