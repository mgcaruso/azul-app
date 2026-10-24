using Azul.Api.DTOs.Auth;
using Azul.Api.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Azul.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("google")]
    [EnableRateLimiting("google-sign-in")]
    [ProducesResponseType<MeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MeDto>> SignInWithGoogle([FromBody] GoogleSignInDto dto)
    {
        return Ok(await authService.SignInWithGoogleAsync(dto.IdToken));
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        await authService.SignOutAsync();
        return NoContent();
    }

    [Authorize]
    [HttpPost("logout-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutEverywhere()
    {
        await authService.SignOutEverywhereAsync(User);
        return NoContent();
    }
}
