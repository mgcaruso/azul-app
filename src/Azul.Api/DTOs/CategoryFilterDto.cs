namespace Azul.Api.DTOs;

public class CategoryFilterDto
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Proveedores distintos, no servicios.
    public int Count { get; set; }
}
