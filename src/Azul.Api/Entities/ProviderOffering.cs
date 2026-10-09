namespace Azul.Api.Entities;

public class ProviderOffering
{
    public int ProviderId { get; set; }
    public Provider Provider { get; set; } = null!;

    public int OfferingId { get; set; }
    public Offering Offering { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
}
