using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Azul.Api.Controllers;

[ApiController]
[Route("api/admin/session")]
public class AdminSessionController : AdminControllerBase
{
    [HttpGet]
    [EnableRateLimiting("admin-login")]
    public IActionResult Get()
    {
        return NoContent();
    }
}