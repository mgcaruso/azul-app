using Azul.Api.DTOs.Categories;

namespace Azul.Api.DTOs.Offerings;

/// <summary>Un servicio con su categoría, para listados donde se mezclan categorías (buscador, panel admin).</summary>
public class OfferingListItemDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required CategoryRefDto Category { get; set; }
}
