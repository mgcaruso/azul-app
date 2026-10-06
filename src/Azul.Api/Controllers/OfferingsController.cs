using Azul.Api.DTOs;
using Azul.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.Controllers;

// Solo el camino feliz: si algo falla, el service lanza una excepción
// y AppExceptionHandler la traduce a 400/404/409/500 con ProblemDetails.
[ApiController]
[Route("api/services")]
public class OfferingsController(IOfferingService offeringService) : ControllerBase
{
    // Reemplaza al GetAll: sin parámetros devuelve todos los servicios, paginados.
    [HttpGet]
    [ProducesResponseType<OfferingSearchResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OfferingSearchResultDto>> Search([FromQuery] OfferingSearchQuery query)
    {
        return Ok(await offeringService.SearchAsync(query));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OfferingDto>> GetById(int id)
    {
        return Ok(await offeringService.GetByIdAsync(id));
    }

    // La "/" inicial hace que la ruta no se sume a "api/services": queda /api/categories/{id}/services.
    [HttpGet("/api/categories/{categoryId:int}/services")]
    [ProducesResponseType<List<OfferingSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<OfferingSummaryDto>>> GetByCategory(int categoryId)
    {
        return Ok(await offeringService.GetByCategoryAsync(categoryId));
    }

    [HttpPost]
    public async Task<ActionResult<OfferingDto>> Create([FromBody] OfferingSaveDto dto)
    {
        var created = await offeringService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] OfferingSaveDto dto)
    {
        await offeringService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        await offeringService.DeleteAsync(id);
        return NoContent();
    }
}
