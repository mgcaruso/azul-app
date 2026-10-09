namespace Azul.Api.Common.Pagination;

// Una página de resultados de cualquier tipo.
// HasMore le dice al front si hay otra página (botón "Ver más" o scroll infinito).
public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasMore { get; set; }
}
