using Azul.Api.Entities;

namespace Azul.Api.DTOs.Providers;

// Una fila de la lista del panel admin.
public class AdminProviderListItemDto
{
    public int Id { get; set; }
    public required string DisplayName { get; set; }
    public ProviderType Type { get; set; }
    public required string PhoneNumber { get; set; }
    public ProviderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
