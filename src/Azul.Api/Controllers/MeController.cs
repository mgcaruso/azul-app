using Azul.Api.Common.Security;
using Azul.Api.DTOs.Auth;
using Azul.Api.DTOs.Providers;
using Azul.Api.Services.Auth;
using Azul.Api.Services.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/me")]
public class MeController(
    IAuthService authService,
    IProviderReadService providerReadService,
    IProviderWriteService providerWriteService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<MeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MeDto>> Get()
    {
        return Ok(await authService.GetMeAsync(User));
    }

    // La ficha de quien ingresó, con su estado (en revisión, publicada...). 404 si todavía no creó una.
    [HttpGet("provider")]
    [ProducesResponseType<MyProviderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MyProviderDto>> GetProvider()
    {
        return Ok(await providerReadService.GetMineAsync(User.GetUserId()));
    }

    // Edit the signed-in user's own ficha. There is no id in the route, so nobody can edit someone else's.
    [HttpPut("provider")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> UpdateProvider([FromBody] ProviderSaveDto dto)
    {
        await providerWriteService.UpdateMineAsync(User.GetUserId(), dto);
        return NoContent();
    }

    // Delete the signed-in user's own ficha.
    [HttpDelete("provider")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteProvider()
    {
        await providerWriteService.DeleteMineAsync(User.GetUserId());
        return NoContent();
    }
}
