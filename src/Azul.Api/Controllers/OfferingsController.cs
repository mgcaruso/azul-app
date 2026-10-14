using Azul.Api.Common.Pagination;
using Azul.Api.DTOs.Offerings;
using Azul.Api.Services.Offerings;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.Controllers;

[ApiController]
[Route("api/offerings")]
public class OfferingsController(IOfferingService offeringService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<OfferingSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<OfferingSummaryDto>>> Search([FromQuery] OfferingSearchQuery query)
    {
        return Ok(await offeringService.SearchAsync(query));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<OfferingDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OfferingDto>> GetById(int id)
    {
        return Ok(await offeringService.GetByIdAsync(id));
    }

    [HttpGet("/api/categories/{categoryId:int}/offerings")]
    [ProducesResponseType<List<OfferingSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<OfferingSummaryDto>>> GetByCategory(int categoryId)
    {
        return Ok(await offeringService.GetByCategoryAsync(categoryId));
    }

    [HttpPost]
    [ProducesResponseType<OfferingDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OfferingDto>> Create([FromBody] OfferingSaveDto dto)
    {
        var created = await offeringService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Update(int id, [FromBody] OfferingSaveDto dto)
    {
        await offeringService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(int id)
    {
        await offeringService.DeleteAsync(id);
        return NoContent();
    }
}
