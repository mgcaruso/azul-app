namespace Azul.Api.Entities;

// Tabla intermedia N:M: qué servicios ofrece cada proveedor.
// La clave es el par (ProviderId, OfferingId): un proveedor no puede tener el mismo servicio dos veces.
// Es una entidad propia (y no un N:M implícito de EF) para poder sumarle columnas más adelante.
public class ProviderOffering
{
    public int ProviderId { get; set; }
    public Provider Provider { get; set; } = null!;

    public int OfferingId { get; set; }
    public Offering Offering { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
}
