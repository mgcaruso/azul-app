using Azul.Api.Common.Pagination;
using Azul.Api.DTOs;
using Azul.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.Controllers;

// Solo el camino feliz: si algo falla, el service lanza una excepción
// y AppExceptionHandler la traduce a 400/404/409/500 con ProblemDetails.
[ApiController]
[Route("api/providers")]
public class ProvidersController(IProviderService providerService) : ControllerBase
{
    // Sin parámetros devuelve todos los proveedores, paginados.
    [HttpGet]
    [ProducesResponseType<PagedResult<ProviderSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ProviderSummaryDto>>> GetAll([FromQuery] ProviderSearchQuery query)
    {
        return Ok(await providerService.GetAllAsync(query));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ProviderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProviderDto>> GetById(int id)
    {
        return Ok(await providerService.GetByIdAsync(id));
    }

    // La "/" inicial hace que la ruta no se sume a "api/providers": queda /api/services/{id}/providers.
    [HttpGet("/api/services/{offeringId:int}/providers")]
    [ProducesResponseType<PagedResult<ProviderSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<ProviderSummaryDto>>> GetByOffering(
        int offeringId, [FromQuery] PageQuery pageQuery)
    {
        return Ok(await providerService.GetByOfferingAsync(offeringId, pageQuery));
    }

    [HttpPost]
    [ProducesResponseType<ProviderDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProviderDto>> Create([FromBody] ProviderSaveDto dto)
    {
        var created = await providerService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
