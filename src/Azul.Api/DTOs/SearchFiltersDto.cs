namespace Azul.Api.DTOs;

public class SearchFiltersDto
{
    // Solo las que tienen al menos un proveedor, por nombre.
    public List<CategoryFilterDto> Categories { get; set; } = [];

    // Servicios cuyo nombre coincide con el texto. Vacío si no hay texto.
    public List<OfferingFilterDto> Offerings { get; set; } = [];
}
