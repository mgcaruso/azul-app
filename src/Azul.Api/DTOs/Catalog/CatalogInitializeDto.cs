using Azul.Api.DTOs.Offerings;
using Azul.Api.DTOs.Providers;

namespace Azul.Api.DTOs.Catalog;

public class CatalogInitializeDto
{
    public List<ProviderSummaryDto> LatestProviders { get; set; } = [];
    public List<OfferingSummaryDto> Offerings { get; set; } = [];
}