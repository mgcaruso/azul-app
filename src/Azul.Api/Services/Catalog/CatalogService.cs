using Azul.Api.DTOs.Catalog;
using Azul.Api.DTOs.Offerings;
using Azul.Api.Services.Offerings;
using Azul.Api.Services.Providers;

namespace Azul.Api.Services.Catalog;

// Arma respuestas que juntan varias entidades usando los otros services (no repite consultas).
public class CatalogService(IOfferingService offeringService, IProviderReadService providerReadService) : ICatalogService
{
    private const int RandomOfferingsLimit = 8;
    private const int MatchingOfferingsLimit = 10;

    // GET /api/catalog/initialize: últimos proveedores + servicios.
    // TODO: cachear esta respuesta (cambia poco y la pide cada visita a la Home). 
    public async Task<CatalogInitializeDto> GetCatalogAsync()
    {
        // Una consulta después de la otra: el DbContext no admite dos a la vez.
        var latestProviders = await providerReadService.GetLatestAsync();
        var offerings = await offeringService.GetRandomAsync(RandomOfferingsLimit);
        return new CatalogInitializeDto { LatestProviders = latestProviders, Offerings = offerings };
    }

    // GET /api/search: la página de proveedores y, si hay texto, los servicios cuyo nombre coincide.
    public async Task<CatalogSearchResponseDto> CatalogSearchAsync(CatalogSearchQuery query)
    {
        var providers = await providerReadService.CatalogSearchAsync(query);

        // Sin texto no se sugieren servicios: SearchAsync de servicios devolvería todos.
        if (string.IsNullOrWhiteSpace(query.Text))
        {
            return new CatalogSearchResponseDto { Providers = providers };
        }

        var offerings = await offeringService.SearchAsync(new OfferingSearchQuery
        {
            Text = query.Text,
            PageSize = MatchingOfferingsLimit
        });

        return new CatalogSearchResponseDto { Providers = providers, Offerings = offerings.Items };
    }
}
