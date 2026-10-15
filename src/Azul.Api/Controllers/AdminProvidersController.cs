using Azul.Api.Common.Pagination;
using Azul.Api.DTOs.Providers;
using Azul.Api.Services.Providers;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.Controllers;

[ApiController]
[Route("api/admin/providers")]
public class AdminProvidersController(
    IProviderReadService providerReadService,
    IProviderWriteService providerWriteService) : AdminControllerBase
{
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(int id, [FromBody] ProviderSaveDto dto)
    {
        await providerWriteService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        await providerWriteService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:int}/publish")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Publish(int id)
    {
        await providerWriteService.PublishAsync(id);
        return NoContent();
    }

    [HttpPost("{id:int}/suspend")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Suspend(int id)
    {
        await providerWriteService.SuspendAsync(id);
        return NoContent();
    }

    [HttpPost("{id:int}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Reject(int id)
    {
        await providerWriteService.RejectAsync(id);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<AdminProviderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminProviderDto>> GetById(int id)
    {
        return await providerReadService.GetAdminByIdAsync(id);
    }

    [HttpGet]
    [ProducesResponseType<PagedResult<AdminProviderListItemDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminProviderListItemDto>>> GetAll(
        [FromQuery] AdminProviderSearchQuery query)
    {
        return await providerReadService.GetAdminListAsync(query);
    }
}
