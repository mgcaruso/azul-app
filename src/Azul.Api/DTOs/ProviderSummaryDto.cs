using Azul.Api.Entities;

namespace Azul.Api.DTOs;

// Versión corta de un proveedor, para listas (los proveedores de un servicio).
// Más adelante acá va lo que muestra la card.
public class ProviderSummaryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ProviderType Type { get; set; }
}
