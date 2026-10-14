using Azul.Api.DTOs.Offerings;
using Azul.Api.Services.Offerings;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.Controllers;

[ApiController]
[Route("api/admin/offerings")]
public class AdminOfferingsController(IOfferingService offeringService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<OfferingDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OfferingDto>> Create([FromBody] OfferingSaveDto dto)
    {
        var created = await offeringService.CreateAsync(dto);
        return CreatedAtAction(nameof(OfferingsController.GetById), "Offerings", new { id = created.Id }, created);
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