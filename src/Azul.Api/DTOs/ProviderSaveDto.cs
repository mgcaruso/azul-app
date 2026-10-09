using System.ComponentModel.DataAnnotations;
using Azul.Api.Entities;

namespace Azul.Api.DTOs;

// Body de POST /api/providers: el proveedor con los ids de los servicios que ofrece.
public class ProviderSaveDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    // Llega como texto: "Individual" o "Business" (JsonStringEnumConverter en Program.cs).
    // ProviderType? para que "no vino" sea null y [Required] lo detecte.
    // [EnumDataType] rechaza números que no son del enum, como "type": 7.
    [Required(ErrorMessage = "El tipo es obligatorio.")]
    [EnumDataType(typeof(ProviderType), ErrorMessage = "El tipo tiene que ser Individual o Business.")]
    public ProviderType? Type { get; set; }

    [Required(ErrorMessage = "Los servicios son obligatorios.")]
    [MinLength(1, ErrorMessage = "Tiene que ofrecer al menos un servicio.")]
    public List<int> OfferingIds { get; set; } = [];
}
