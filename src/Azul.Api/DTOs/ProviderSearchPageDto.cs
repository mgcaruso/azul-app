using Azul.Api.Common.Pagination;

namespace Azul.Api.DTOs;

// Como PagedResult, más el total para el título y el botón "Ver N resultados".
public class ProviderSearchPageDto : PagedResult<ProviderSummaryDto>
{
    public int Total { get; set; }
}
