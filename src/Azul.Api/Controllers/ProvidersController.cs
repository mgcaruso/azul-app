using Azul.Api.Common.Pagination;
using Azul.Api.DTOs.Providers;
using Azul.Api.Services.Providers;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.Controllers;

[ApiController]
[Route("api/providers")]
public class ProvidersController(IProviderReadService providerReadService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<ProviderSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ProviderSummaryDto>>> GetAll([FromQuery] ProviderSearchQuery query)
    {
        return Ok(await providerReadService.GetAllAsync(query));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ProviderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProviderDto>> GetById(int id)
    {
        return Ok(await providerReadService.GetByIdAsync(id));
    }

    [HttpGet("/api/offerings/{offeringId:int}/providers")]
    [ProducesResponseType<PagedResult<ProviderSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<ProviderSummaryDto>>> GetByOffering(
        int offeringId, [FromQuery] PageQuery pageQuery)
    {
        return Ok(await providerReadService.GetByOfferingAsync(offeringId, pageQuery));
    }

    [HttpGet("featured")]
    [ProducesResponseType<List<ProviderSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProviderSummaryDto>>> GetFeatured()
    {
        return Ok(await providerReadService.GetFeaturedAsync());
    }
    
}
