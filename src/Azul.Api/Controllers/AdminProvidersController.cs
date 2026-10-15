using Azul.Api.DTOs.Providers;
using Azul.Api.Services.Providers;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.Controllers;

[ApiController]
[Route("api/admin/providers")]
public class AdminProvidersController(IProviderWriteService providerWriteService) : AdminControllerBase
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
    
    [HttpPut("publish/{id:int}")]
    public async Task<ActionResult> Publish(int id)
    {
        await providerWriteService.DeleteAsync(id);
        return NoContent();
    }
}
