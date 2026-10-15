using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.Controllers;

[ApiController]
[Route("api/admin/session")]
public class AdminSessionController : AdminControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return  NoContent();
    }
}