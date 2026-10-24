using Azul.Api.DTOs.Auth;
using Azul.Api.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/me")]
public class MeController(IAuthService authService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<MeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MeDto>> Get()
    {
        return Ok(await authService.GetMeAsync(User));
    }
}
