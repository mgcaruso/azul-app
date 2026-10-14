using Azul.Api.Common.Pagination;
using Azul.Api.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.DTOs.Search;

// GET /api/search?text=&category=&category=&providerType=&offering=&sortBy=&page=&pageSize=
public class SearchQuery : PageQuery
{
    public string Text { get; set; } = string.Empty;

    // Se repite en la URL (?category=3&category=4). Entre categorías es "o".
    [FromQuery(Name = "category")]
    public List<int> CategoryIds { get; set; } = [];

    public ProviderType? ProviderType { get; set; }
    

    public ProviderSort SortBy { get; set; } = ProviderSort.Featured;
    
    
}
