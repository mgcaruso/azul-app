namespace Azul.Api.DTOs;

public class CatalogInitializeDto
{
    public List<ProviderSummaryDto> LatestProviders { get; set; } = [];
    public List<OfferingSummaryDto> Offerings { get; set; } = [];
}