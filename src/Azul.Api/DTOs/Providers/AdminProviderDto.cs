using Azul.Api.Entities;

namespace Azul.Api.DTOs.Providers;

// Detalle de un proveedor para el panel admin: lo mismo que ve el público, más el estado y las fechas.
public class AdminProviderDto : ProviderDto
{
    // Para precargar el formulario de edición: el PUT reemplaza estos campos, y sin ellos se perderían.
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhotoKey { get; set; }

    public ProviderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FirstPublishedAt { get; set; }
}
