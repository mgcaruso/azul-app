using Azul.Api.DTOs;

namespace Azul.Api.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto> GetByIdAsync(int id);
    Task<CategoryDto> CreateAsync(CategorySaveDto categorySaveDto);
    Task UpdateAsync(int id, CategorySaveDto categorySaveDto);
    Task DeleteAsync(int id);
}
