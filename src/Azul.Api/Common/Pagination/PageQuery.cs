using System.ComponentModel.DataAnnotations;

namespace Azul.Api.Common.Pagination;

// Parámetros de paginado que llegan por query (?page=&pageSize=).
// Los DTOs de búsqueda heredan de esta clase y suman sus propios filtros.
public class PageQuery
{
    // Tope en 10.000 para que (Page - 1) * PageSize no se pase del máximo de int.
    [Range(1, 10_000, ErrorMessage = "La página tiene que estar entre 1 y 10000.")]
    public int Page { get; set; } = 1;

    [Range(1, 50, ErrorMessage = "El tamaño de página tiene que estar entre 1 y 50.")]
    public int PageSize { get; set; } = 12;
}
