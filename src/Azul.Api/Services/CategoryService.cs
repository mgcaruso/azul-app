using System.Linq.Expressions;
using Azul.Api.Data;
using Azul.Api.DTOs;
using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Azul.Api.Services;

public class CategoryService(AppDbContext dbContext) : ICategoryService
{
    // Mapeo entidad -> DTO. Es una Expression (no un método) para que EF lo traduzca a SQL
    // y solo traiga las columnas que usa el DTO.
    private static readonly Expression<Func<Category, CategoryDto>> ToDto =
        c => new CategoryDto { Id = c.Id, Name = c.Name };

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await dbContext.Categories
            .OrderBy(c => c.Name)
            .Select(ToDto)
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var result = await dbContext.Categories
            .Where(c => c.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();
        return result;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deletedRows = await dbContext.Categories
            .Where(c => c.Id == id)
            .ExecuteDeleteAsync();

        return deletedRows > 0;
    }

    public async Task<CategoryDto?> CreateAsync(CategorySaveDto categorySaveDto)
    {
        var name = categorySaveDto.Name.Trim();
        if (await NameExists(name))
        {
            return null;
        }

        var category = new Category { Name = name };
        dbContext.Add(category);
        if (!await TrySaveChanges())
        {
            return null;
        }

        return new CategoryDto { Id = category.Id, Name = category.Name };
    }

    public async Task<UpdateCategoryResult> UpdateAsync(int id, CategorySaveDto categorySaveDto)
    {
        var categoryDb = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (categoryDb == null)
        {
            return UpdateCategoryResult.NotFound;
        }

        var name = categorySaveDto.Name.Trim();
        if (await NameExists(name, excludeId: id))
        {
            return UpdateCategoryResult.DuplicateName;
        }

        categoryDb.Name = name;
        if (!await TrySaveChanges())
        {
            return UpdateCategoryResult.DuplicateName;
        }

        return UpdateCategoryResult.Updated;
    }

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
