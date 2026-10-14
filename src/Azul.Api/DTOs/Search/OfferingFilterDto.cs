namespace Azul.Api.DTOs.Search;

public class OfferingFilterDto
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Proveedores que lo ofrecen.
    public int Count { get; set; }
}
