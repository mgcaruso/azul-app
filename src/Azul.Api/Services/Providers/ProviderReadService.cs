using Azul.Api.Common.Exceptions;
using Azul.Api.Common.Pagination;
using Azul.Api.DTOs.Catalog;
using Azul.Api.DTOs.Providers;
using Azul.Api.Data;
using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Azul.Api.Services.Providers;

// Lecturas de proveedores: listados, detalle, home y búsqueda. No modifica la base.
public class ProviderReadService(AppDbContext dbContext, IConfiguration configuration) : IProviderReadService
{
    // En la base va la key; al front se le devuelve la URL armada con la dirección base.
    private readonly string photoBaseUrl = configuration["Photos:BaseUrl"]
        ?? throw new InvalidOperationException("Falta la configuración Photos:BaseUrl.");

    // GET /api/providers?type=&page=&pageSize=
    public async Task<PagedResult<ProviderSummaryDto>> GetAllAsync(ProviderSearchQuery query)
    {
        return await GetPublishedProviders()
            .WhereType(query.Type)
            .OrderBy(p => p.DisplayName)
            .ThenBy(p => p.Id)
            .Select(ProviderProjections.ToSummaryDto(photoBaseUrl))
            .PaginateAsync(query);
    }

    // GET /api/admin/providers?status=&text=&type=&page=&pageSize=: todos los estados.
    public async Task<PagedResult<AdminProviderListItemDto>> GetAdminListAsync(AdminProviderSearchQuery query)
    {
        return await dbContext.Providers
            .WhereType(query.Type)
            .WhereInStatuses(query.Statuses)
            .WhereMatchesText(query.Text)
            .OrderByDescending(p => p.CreatedAt)
            .ThenBy(p => p.Id)
            .Select(ProviderProjections.ToAdminListItemDto())
            .PaginateAsync(query);
    }

    // GET /api/admin/providers/{id}: cualquier estado.
    public async Task<AdminProviderDto> GetAdminByIdAsync(int id)
    {
        return await dbContext.Providers
                   .Where(p => p.Id == id)
                   .Select(ProviderProjections.ToAdminDto(photoBaseUrl))
                   .FirstOrDefaultAsync()
               ?? throw new NotFoundException("provider", id);
    }

    // GET /api/providers/{id}: solo proveedores publicados.
    public async Task<ProviderDto> GetByIdAsync(int id)
    {
        return await dbContext.Providers
                   .Where(p => p.Id == id && p.Status == ProviderStatus.Published)
                   .Select(ProviderProjections.ToDto(photoBaseUrl))
                   .FirstOrDefaultAsync()
               ?? throw new NotFoundException("provider", id);
    }


    // GET /api/offerings/{offeringId}/providers?page=&pageSize=
    public async Task<PagedResult<ProviderSummaryDto>> GetByOfferingAsync(int offeringId, PageQuery pageQuery)
    {
        var offeringDb = await dbContext.Offerings.AnyAsync(o => o.Id == offeringId);
        if (!offeringDb)
        {
            throw new NotFoundException("offering", offeringId);
        }

        var providersQuery = GetPublishedProviders()
            .Where(p => p.Offerings.Any(po => po.OfferingId == offeringId));

        return await providersQuery
            .OrderBy(p => p.DisplayName)
            .ThenBy(p => p.Id)
            .Select(ProviderProjections.ToSummaryDto(photoBaseUrl))
            .PaginateAsync(pageQuery);
    }

    // Publicados + filtros de la request. Cada filtro que viene suma un Where ("y").
    // Para texto: SearchText.Matches(columna, LikePattern.Contains(texto)).
    private IQueryable<Provider> GetPublishedProvidersFiltered(CatalogSearchQuery query)
    {
        return GetPublishedProviders()
            .WhereMatchesText(query.Text)
            .WhereInCategories(query.CategoryIds)
            .WhereType(query.ProviderType);
    }

  
    // GET /api/search: la página de proveedores.
    // Total = todos los que cumplen los filtros de la request; Items ordenados según SortBy y paginados;
    // HasMore = Page * PageSize < Total.
    public async Task<ProviderSearchPageDto> CatalogSearchAsync(CatalogSearchQuery query)
    {
        var filteredProviders = GetPublishedProvidersFiltered(query);

        // Total de todos los que cumplen los filtros, antes de paginar.
        var total = await filteredProviders.CountAsync();

        // El orden va antes del Select.
        var page = await filteredProviders
            .OrderBySort(query.SortBy)
            .Select(ProviderProjections.ToSummaryDto(photoBaseUrl))
            .PaginateAsync(query);

        return new ProviderSearchPageDto
        {
            Items = page.Items,
            Page = page.Page,
            PageSize = page.PageSize,
            HasMore = page.HasMore,
            Total = total
        };
    }

    private IQueryable<Provider> GetPublishedProviders()
    {
        return dbContext.Providers.Where(p => p.Status == ProviderStatus.Published);
    }

    public async Task<List<ProviderSummaryDto>> GetLatestAsync()
    {
        var providersDb = await GetPublishedProviders()
            .OrderByDescending(p => p.FirstPublishedAt)
            .ThenByDescending(p => p.Id)
            .Take(6)
            .Select(ProviderProjections.ToSummaryDto(photoBaseUrl))
            .ToListAsync();
        return providersDb;
    }

    public async Task<List<ProviderSummaryDto>> GetFeaturedAsync()
    {
        var providersDb = await GetPublishedProviders()
            .Where(ProviderProfile.IsComplete)
            .OrderBy(p => p.DisplayName)
            .ThenBy(p => p.Id)
            .Take(6)
            .Select(ProviderProjections.ToSummaryDto(photoBaseUrl))
            .ToListAsync();
        return providersDb;
    }
}
