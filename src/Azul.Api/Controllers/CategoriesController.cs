using System.Linq.Expressions;
using Azul.Api.Data;
using Azul.Api.DTOs;
using Azul.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Azul.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(AppDbContext dbContext) : ControllerBase
{
    private const string DuplicateNameMessage = "Ya existe una categoria con ese nombre";

    // Mapeo entidad -> DTO. Es una Expression (no un método) para que EF lo traduzca a SQL
    // y solo traiga las columnas que usa el DTO.
    private static readonly Expression<Func<Category, CategoryDto>> ToDto =
        c => new CategoryDto { Id = c.Id, Name = c.Name };

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        var result = await dbContext.Categories
            .OrderBy(c => c.Name)
            .Select(ToDto)
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var result = await dbContext.Categories
            .Where(c => c.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();

        if (result == null)
        {
            return NotFound("La categoria no existe");
        }

        return Ok(result);
    }
    
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CategorySaveDto dto)
    {
        var name = dto.Name.Trim();
        if (await NameExists(name))
        {
            return Conflict(DuplicateNameMessage);
        }

        var category = new Category { Name = name };
        dbContext.Add(category);
        if (!await TrySaveChanges())
        {
            return Conflict(DuplicateNameMessage);
        }

        var categoryDto = new CategoryDto { Id = category.Id, Name = category.Name };
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, categoryDto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] CategorySaveDto dto)
    {
        var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
        {
            return NotFound("La categoria no existe");
        }

        var name = dto.Name.Trim();
        if (await NameExists(name, excludeId: id))
        {
            return Conflict(DuplicateNameMessage);
        }

        category.Name = name;
        if (!await TrySaveChanges())
        {
            return Conflict(DuplicateNameMessage);
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deletedRows = await dbContext.Categories
            .Where(c => c.Id == id)
            .ExecuteDeleteAsync();

        if (deletedRows == 0)
        {
            return NotFound("La categoria no existe");
        }

        return NoContent();
    }

    // ¿Hay otra categoría con este nombre (sin distinguir mayúsculas)?
    // excludeId sirve en el PUT, para no chocar con la categoría que se está editando.
    private Task<bool> NameExists(string name, int? excludeId = null)
    {
        return dbContext.Categories
            .AnyAsync(c => c.Id != excludeId && c.Name.ToLower() == name.ToLower());
    }

    // Guarda los cambios. Si el índice único de la base rechaza el nombre
    // (dos pedidos a la vez que pasaron el NameExists), devuelve false en vez de tirar un 500.
    private async Task<bool> TrySaveChanges()
    {
        try
        {
            await dbContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return false;
        }
    }
}