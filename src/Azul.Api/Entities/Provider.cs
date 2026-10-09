using System.ComponentModel.DataAnnotations;

namespace Azul.Api.Entities;

// La ficha pública de un proveedor. Por ahora solo el nombre:
// contacto, estado, verificación y demás se suman en iteraciones siguientes.
public class Provider
{
    public int Id { get; set; }

    [MaxLength(100)]
    public required string Name { get; set; }

    public ProviderType Type { get; set; }

    // Los servicios que ofrece, a través de la tabla intermedia.
    public List<ProviderOffering> Offerings { get; set; } = [];
}
