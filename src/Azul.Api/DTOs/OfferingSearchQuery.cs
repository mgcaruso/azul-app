using System.ComponentModel.DataAnnotations;
using Azul.Api.Common.Pagination;

namespace Azul.Api.DTOs;

// Parámetros de GET /api/services?text=&categoryId=&page=&pageSize=
// Todos opcionales: sin ninguno, devuelve todos los servicios paginados.
// Page y PageSize vienen de PageQuery.
public class OfferingSearchQuery : PageQuery
{
    [MaxLength(100, ErrorMessage = "El texto de búsqueda no puede superar los 100 caracteres.")]
    public string? Text { get; set; }

    public int? CategoryId { get; set; }
}
