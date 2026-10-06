using System.ComponentModel.DataAnnotations;

namespace Azul.Api.DTOs;

// Parámetros de GET /api/services?text=&categoryId=&page=&pageSize=
// Todos opcionales: sin ninguno, devuelve todos los servicios paginados.
public class OfferingSearchQuery
{
    [MaxLength(100, ErrorMessage = "El texto de búsqueda no puede superar los 100 caracteres.")]
    public string? Text { get; set; }

    public int? CategoryId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La página tiene que ser 1 o mayor.")]
    public int Page { get; set; } = 1;

    [Range(1, 50, ErrorMessage = "El tamaño de página tiene que estar entre 1 y 50.")]
    public int PageSize { get; set; } = 12;
}
