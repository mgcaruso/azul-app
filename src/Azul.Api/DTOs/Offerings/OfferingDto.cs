namespace Azul.Api.DTOs.Offerings;

public class OfferingDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public  int CategoryId { get; set; }
}