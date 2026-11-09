using Azul.Api.DTOs.Providers;

namespace Azul.Api.Services.Providers;

public interface IProviderWriteService
{
    Task<ProviderDto> CreateAsync(Guid userId, ProviderSaveDto providerSaveDto);
    Task UpdateAsync(int id, ProviderSaveDto providerSaveDto);
    Task DeleteAsync(int id);

    // The signed-in user's own ficha (/api/me/provider).
    Task UpdateMineAsync(Guid userId, ProviderSaveDto providerSaveDto);
    Task DeleteMineAsync(Guid userId);
    
    Task PublishAsync(int id);
    Task SuspendAsync(int id);
    Task RejectAsync(int id);
}
