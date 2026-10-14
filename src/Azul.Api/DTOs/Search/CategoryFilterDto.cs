namespace Azul.Api.DTOs.Search;

public class CategoryFilterDto
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Proveedores distintos, no servicios.
    public int Count { get; set; }
}
