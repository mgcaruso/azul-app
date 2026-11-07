using Azul.Api.DTOs.Administrators;

namespace Azul.Api.Services.Administrators;

public interface IAdministratorService
{
    Task<List<AdministratorDto>> GetAllAsync();
    Task AddAsync(string email);
    Task RemoveAsync(Guid userId, Guid currentUserId);
}