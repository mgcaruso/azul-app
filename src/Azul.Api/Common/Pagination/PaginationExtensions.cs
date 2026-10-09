using Microsoft.EntityFrameworkCore;

namespace Azul.Api.Common.Pagination;

public static class PaginationExtensions
{
    public static async Task<PagedResult<T>> PaginateAsync<T>(this IQueryable<T> query, PageQuery pageQuery)
    {
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
