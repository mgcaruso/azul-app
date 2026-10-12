namespace Azul.Api.DTOs;

public class OfferingFilterDto
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Proveedores que lo ofrecen.
    public int Count { get; set; }
}
