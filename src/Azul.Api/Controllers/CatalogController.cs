using Azul.Api.DTOs.Catalog;
using Azul.Api.DTOs.Search;
using Azul.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.Controllers;

// Endpoints que combinan varias entidades para explorar el catálogo (Home, buscador principal).
// Lo que pertenece a una sola entidad va en el controller de esa entidad.
[ApiController]
[Route("api/catalog")]
public class CatalogController(ICatalogService catalogService) : ControllerBase
{
    // GET /api/catalog/initialize
    [HttpGet("initialize")]
    [ProducesResponseType<CatalogInitializeDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CatalogInitializeDto>> GetCatalog()
    {
        return Ok(await catalogService.GetCatalogAsync());
    }

    // GET /api/search?text=torta&category=3&category=4&providerType=Business&sortBy=Featured&page=1&pageSize=12
    [HttpGet("/api/search")]
    [ProducesResponseType<SearchResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchResponseDto>> Search([FromQuery] SearchQuery query)
    {
        return Ok(await catalogService.GetProvidersCatalogAsync(query));
    }
}
