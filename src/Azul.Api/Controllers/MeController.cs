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
public class MeController(IAuthService authService, IProviderReadService providerReadService) : ControllerBase
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
}
