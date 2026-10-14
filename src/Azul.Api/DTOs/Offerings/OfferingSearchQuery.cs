using Azul.Api.Common.Pagination;

namespace Azul.Api.DTOs.Offerings;

public class OfferingSearchQuery : PageQuery
{
    public string? Text { get; set; }

    public int? CategoryId { get; set; }
}
