using Azul.Api.Common.Pagination;
using Azul.Api.DTOs;
using Azul.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.Controllers;

[ApiController]
[Route("api/providers")]
public class ProvidersController(IProviderService providerService) : ControllerBase
{
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

    [HttpGet("/api/offerings/{offeringId:int}/providers")]
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
