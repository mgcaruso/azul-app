namespace Azul.Api.DTOs.Providers;

// Un grupo de proveedores que comparten el mismo celular o el mismo nombre.
public class DuplicateGroupDto
{
    // "phone" (mismo celular), "name" (mismo nombre) o "both" (las dos cosas a la vez).
    public required string Reason { get; set; }

    // Lo que tienen en común: el celular o el nombre (con "both", el celular).
    public required string Value { get; set; }

    public List<DuplicateCandidateDto> Providers { get; set; } = [];
}
