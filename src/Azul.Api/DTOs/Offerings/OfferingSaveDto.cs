namespace Azul.Api.DTOs.Offerings;

public class OfferingSaveDto
{
    public string Name { get; set; } = string.Empty;

    public int? CategoryId { get; set; }

    // Null keeps the current keywords on update (and means none on create).
    public List<string>? Keywords { get; set; }
}
