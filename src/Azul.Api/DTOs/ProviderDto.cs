using Azul.Api.Entities;

namespace Azul.Api.DTOs;

// El proveedor completo, para GET /api/providers/{id}.
public class ProviderDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ProviderType Type { get; set; }
    public List<OfferingSummaryDto> Offerings { get; set; } = [];
}
