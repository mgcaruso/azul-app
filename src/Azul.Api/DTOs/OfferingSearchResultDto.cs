namespace Azul.Api.DTOs;

// Una página de resultados. HasMore le dice al front si hay otra página (botón "Ver más").
public class OfferingSearchResultDto
{
    public List<OfferingSummaryDto> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasMore { get; set; }
}
