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
    [ProducesResponseType<PagedResult<OfferingListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<OfferingListItemDto>>> Search([FromQuery] OfferingSearchQuery query)
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
}
