using Azul.Api.Entities;

namespace Azul.Api.DTOs.Providers;

// Detalle de un proveedor para el panel admin: lo mismo que ve el público, más el estado y las fechas.
public class AdminProviderDto : ProviderDto
{
    public ProviderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FirstPublishedAt { get; set; }
}
