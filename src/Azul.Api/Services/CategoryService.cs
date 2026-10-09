using System.Linq.Expressions;
using Azul.Api.Common.Exceptions;
using Azul.Api.Data;
using Azul.Api.DTOs;
using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Azul.Api.Services;

public class CategoryService(AppDbContext dbContext) : ICategoryService
{
    private static readonly Expression<Func<Category, CategoryDto>> ToDto =
        c => new CategoryDto { Id = c.Id, Name = c.Name };

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await dbContext.Categories
            .OrderBy(c => c.Name)
            .Select(ToDto)
            .ToListAsync();
    }

    public async Task<CategoryDto> GetByIdAsync(int id)
    {
        return await dbContext.Categories
                   .Where(c => c.Id == id)
                   .Select(ToDto)
                   .FirstOrDefaultAsync()
               ?? throw new NotFoundException("category", id);
    }

    public async Task<CategoryDto> CreateAsync(CategorySaveDto categorySaveDto)
    {
        var name = categorySaveDto.Name.Trim();
        if (await NameExists(name))
        {
            throw ConflictException.Duplicate("name");
        }

        var category = new Category { Name = name };
        dbContext.Add(category);
        await dbContext.SaveChangesAsync();

        return new CategoryDto { Id = category.Id, Name = category.Name };
    }

    public async Task UpdateAsync(int id, CategorySaveDto categorySaveDto)
    {
        var category = await dbContext.Categories.FindAsync(id)
                       ?? throw new NotFoundException("category", id);

        var name = categorySaveDto.Name.Trim();
        if (await NameExists(name, excludeId: id))
        {
            throw ConflictException.Duplicate("name");
        }

        category.Name = name;
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var deletedRows = await dbContext.Categories
            .Where(c => c.Id == id)
            .ExecuteDeleteAsync();

        if (deletedRows == 0)
        {
            throw new NotFoundException("category", id);
        }
    }

    private Task<bool> NameExists(string name, int? excludeId = null)
    {
        var lowerName = name.ToLowerInvariant();

        return dbContext.Categories
            .AnyAsync(c => c.Id != excludeId
                           && AppDbContext.FUnaccent(c.Name.ToLower()) == AppDbContext.FUnaccent(lowerName));
    }
}
