using Azul.Api.Entities;

namespace Azul.Api.DTOs.Providers;

public class DuplicateCandidateDto
{
    public int Id { get; set; }
    public required string DisplayName { get; set; }
    public ProviderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    // Solo en el detalle de un proveedor: por qué es posible duplicado ("phone" o "name").
    // En los grupos de duplicados no se usa, porque el motivo está en el grupo.
    public string? Reason { get; set; }
}
