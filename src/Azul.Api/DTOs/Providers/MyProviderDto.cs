using Azul.Api.Entities;

namespace Azul.Api.DTOs.Providers;

// La ficha vista por su dueña: lo público más el estado de la revisión.
public class MyProviderDto : ProviderDto
{
    public ProviderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FirstPublishedAt { get; set; }
}
