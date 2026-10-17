using Azul.Api.Common.Exceptions;
using Azul.Api.Common.Pagination;
using Azul.Api.DTOs.Providers;
using Azul.Api.Services.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Azul.Api.Controllers;

[ApiController]
[Route("api/providers")]
public class ProvidersController(
    IProviderReadService providerReadService,
    IProviderWriteService providerWriteService) : ControllerBase
{
    private const int MaxPhotoBytes = 5 * 1024 * 1024;

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

    // Alta pública ("Sumá tu negocio"): siempre queda en estado Pending, el admin la revisa y la publica.
    [HttpPost]
    [EnableRateLimiting("provider-signup")]
    [ProducesResponseType<ProviderDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProviderDto>> Create([FromBody] ProviderSaveDto dto)
    {
        var created = await providerWriteService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{clientId:guid}/photo")]
    [EnableRateLimiting("photo-upload")]
    [RequestSizeLimit(MaxPhotoBytes)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadPhoto(Guid clientId, IFormFile photo, CancellationToken ct)
    {
        if (photo.Length > MaxPhotoBytes)
        {
            throw ValidationFailedException.ForField("photo", "La foto puede pesar hasta 5 MB.");
        }

        await using var stream = photo.OpenReadStream();
        await providerWriteService.SavePendingPhotoAsync(clientId, stream, ct);
        return NoContent();
    }

    [HttpGet("featured")]
    [ProducesResponseType<List<ProviderSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProviderSummaryDto>>> GetFeatured()
    {
        return Ok(await providerReadService.GetFeaturedAsync());
    }
    
}
