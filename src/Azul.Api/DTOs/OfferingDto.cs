namespace Azul.Api.DTOs;

public class OfferingDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public  int CategoryId { get; set; }
}