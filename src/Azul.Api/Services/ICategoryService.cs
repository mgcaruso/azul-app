using Azul.Api.DTOs;

namespace Azul.Api.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<CategoryDto?> CreateAsync(CategorySaveDto categorySaveDto);
    Task<UpdateCategoryResult> UpdateAsync(int id, CategorySaveDto categorySaveDto);
    Task<bool> DeleteAsync(int id);
}
