using Azul.Api.Common.Security;
using Azul.Api.DTOs.Administrators;
using Azul.Api.Services.Administrators;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.Controllers;

[ApiController]
[Route("api/admin/administrators")]
public class AdminAdministratorsController(IAdministratorService administratorService) : AdminControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<AdministratorDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AdministratorDto>>> GetAll()
    {
        return Ok(await administratorService.GetAllAsync());
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Add([FromBody] AdministratorAddDto dto)
    {
        await administratorService.AddAsync(dto.Email);
        return NoContent();
    }

    [HttpDelete("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Remove(Guid userId)
    {
        await administratorService.RemoveAsync(userId, User.GetUserId());
        return NoContent();
    }
}
