using Microsoft.EntityFrameworkCore;

namespace Azul.Api.Common.Pagination;

public static class PaginationExtensions
{
    // Pagina cualquier IQueryable: query.OrderBy(...).Select(...).PaginateAsync(pageQuery).
    // Importante: la query tiene que venir ORDENADA, si no la base puede devolver
    // las filas en cualquier orden y una misma fila aparecer en dos páginas.
    public static async Task<PagedResult<T>> PaginateAsync<T>(this IQueryable<T> query, PageQuery pageQuery)
    {
        // Se pide una fila de más: si vino, hay otra página. Así no hace falta un COUNT(*).
        var items = await query
            .Skip((pageQuery.Page - 1) * pageQuery.PageSize)
            .Take(pageQuery.PageSize + 1)
            .ToListAsync();

        var hasMore = items.Count > pageQuery.PageSize;
        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
        }

        return new PagedResult<T>
        {
            Items = items,
            Page = pageQuery.Page,
            PageSize = pageQuery.PageSize,
            HasMore = hasMore
        };
    }
}
