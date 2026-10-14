using Azul.Api.DTOs.Categories;
using Azul.Api.Services.Categories;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.Controllers;

[ApiController]
[Route("api/admin/categories")]
public class AdminCategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CategoryDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CategorySaveDto dto)
    {
        var created = await categoryService.CreateAsync(dto);
        return CreatedAtAction(nameof(CategoriesController.GetById), "Categories", new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Update(int id, [FromBody] CategorySaveDto dto)
    {
        await categoryService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(int id)
    {
        await categoryService.DeleteAsync(id);
        return NoContent();
    }
}