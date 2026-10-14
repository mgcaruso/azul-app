namespace Azul.Api.DTOs.Categories;

public class CategoryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    
    public int OfferingsCount { get; set; }
}