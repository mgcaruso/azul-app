using Azul.Api.Common.Pagination;
using Azul.Api.Entities;

namespace Azul.Api.DTOs;

public class ProviderSearchQuery : PageQuery
{
    public ProviderType? Type { get; set; }
}
