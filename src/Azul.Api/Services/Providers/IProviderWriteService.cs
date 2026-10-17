using Azul.Api.DTOs.Providers;

namespace Azul.Api.Services.Providers;

public interface IProviderWriteService
{
    Task<ProviderDto> CreateAsync(ProviderSaveDto providerSaveDto);
    Task UpdateAsync(int id, ProviderSaveDto providerSaveDto);
    Task DeleteAsync(int id);
    Task SavePendingPhotoAsync(Guid clientId, Stream photo, CancellationToken ct);
    
    Task PublishAsync(int id);
    Task SuspendAsync(int id);
    Task RejectAsync(int id);
}
