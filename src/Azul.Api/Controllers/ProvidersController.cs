using System.Security.Claims;
using Azul.Api.Common.Pagination;
using Azul.Api.Common.Security;
using Azul.Api.DTOs.Providers;
using Azul.Api.Services.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Azul.Api.Controllers;

[ApiController]
[Route("api/providers")]
public class ProvidersController(
    IProviderReadService providerReadService,
    IProviderWriteService providerWriteService) : ControllerBase
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

    // Alta de proveedor ("Sumá tu negocio"): siempre queda en estado Pending, el admin la revisa y la publica.
    [HttpPost]
    [Authorize]
    [EnableRateLimiting("provider-signup")]
    [ProducesResponseType<ProviderDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] 
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProviderDto>> Create([FromBody] ProviderSaveDto dto)
    {
        // user id obtenido de la cookie
        var userId = User.GetUserId();
        var created = await providerWriteService.CreateAsync(userId, dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("featured")]
    [ProducesResponseType<List<ProviderSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProviderSummaryDto>>> GetFeatured()
    {
        return Ok(await providerReadService.GetFeaturedAsync());
    }
    
}
