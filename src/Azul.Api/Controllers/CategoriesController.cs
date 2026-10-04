using Azul.Api.DTOs;
using Azul.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.Controllers;

// Solo el camino feliz: si algo falla, el service lanza una excepción
// y AppExceptionHandler la traduce a 404/409/500 con ProblemDetails.
[ApiController]
[Route("api/categories")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        return Ok(await categoryService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        return Ok(await categoryService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CategorySaveDto dto)
    {
        var created = await categoryService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] CategorySaveDto dto)
    {
        await categoryService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        await categoryService.DeleteAsync(id);
        return NoContent();
    }
}
