using System.ComponentModel.DataAnnotations;

namespace Azul.Api.DTOs;

public class OfferingSaveDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres.")]
    public string? Description { get; set; }

    // int? para que "no vino" sea null y [Required] lo detecte (un int valdría 0).
    [Required(ErrorMessage = "La categoría es obligatoria.")]
    public int? CategoryId { get; set; }
}