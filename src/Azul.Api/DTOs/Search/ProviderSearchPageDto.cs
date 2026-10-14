using Azul.Api.Common.Pagination;
using Azul.Api.DTOs.Providers;

namespace Azul.Api.DTOs.Search;

// Como PagedResult, más el total para el título y el botón "Ver N resultados".
public class ProviderSearchPageDto : PagedResult<ProviderSummaryDto>
{
    public int Total { get; set; }
}
