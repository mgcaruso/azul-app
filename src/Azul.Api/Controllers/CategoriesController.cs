using Azul.Api.DTOs;
using Azul.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    private const string NotFoundMessage = "La categoria no existe";
    private const string DuplicateNameMessage = "Ya existe una categoria con ese nombre";

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        var result = await categoryService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var result = await categoryService.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound(NotFoundMessage);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CategorySaveDto dto)
    {
        var created = await categoryService.CreateAsync(dto);
        if (created == null)
        {
            return Conflict(DuplicateNameMessage);
        }

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] CategorySaveDto dto)
    {
        var result = await categoryService.UpdateAsync(id, dto);

        return result switch
        {
            UpdateCategoryResult.NotFound => NotFound(NotFoundMessage),
            UpdateCategoryResult.DuplicateName => Conflict(DuplicateNameMessage),
            _ => NoContent()
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await categoryService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(NotFoundMessage);
        }

        return NoContent();
    }
}
