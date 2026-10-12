using System.Linq.Expressions;
using Azul.Api.Common.Exceptions;
using Azul.Api.Common.Pagination;
using Azul.Api.Common.Search;
using Azul.Api.Data;
using Azul.Api.DTOs;
using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Azul.Api.Services;

public class ProviderService(AppDbContext dbContext, IPhoneHasher phoneHasher, IConfiguration configuration)
    : IProviderService
{
    private const string InstagramUniqueIndex = "IX_Providers_InstagramUsername";
    private const int OfferingFiltersLimit = 10;

    // En la base va la key; al front se le devuelve la URL armada con la dirección base.
    private readonly string photoBaseUrl = configuration["Photos:BaseUrl"]
        ?? throw new InvalidOperationException("Falta la configuración Photos:BaseUrl.");

    private Expression<Func<Provider, ProviderSummaryDto>> ToProviderSummaryDto =>
        p => new ProviderSummaryDto
        {
            Id = p.Id,
            DisplayName = p.DisplayName,
            Type = p.Type,
            PhotoUrl = p.PhotoKey == null ? null : photoBaseUrl + p.PhotoKey,
            Description = p.Description,
            PhoneNumber = p.PhoneNumber,
            InstagramUsername = p.InstagramUsername,
            CategoryName = p.Offerings
                .OrderBy(po => po.Offering.Name)
                .Select(po => po.Offering.Category.Name)
                .FirstOrDefault(),
            TopOfferings = p.Offerings
                .OrderBy(po => po.Offering.Name)
                .Take(2)
                .Select(po => new OfferingSummaryDto { Id = po.Offering.Id, Name = po.Offering.Name })
                .ToList(),
            OfferingCount = p.Offerings.Count
        };

    private Expression<Func<Provider, ProviderDto>> ToProviderDto =>
        p => new ProviderDto
        {
            Id = p.Id,
            DisplayName = p.DisplayName,
            Type = p.Type,
            Description = p.Description,
            PhotoUrl = p.PhotoKey == null ? null : photoBaseUrl + p.PhotoKey,
            PhoneNumber = p.PhoneNumber,
            InstagramUsername = p.InstagramUsername,
            Address = p.Address,
            Hours = p.Hours,
            Offerings = p.Offerings
                .OrderBy(po => po.Offering.Name)
                .Select(po => new OfferingSummaryDto { Id = po.Offering.Id, Name = po.Offering.Name })
                .ToList()
        };

    // GET /api/providers?type=&page=&pageSize=
    public async Task<PagedResult<ProviderSummaryDto>> GetAllAsync(ProviderSearchQuery query)
    {
        IQueryable<Provider> providerQueryable = dbContext.Providers
            .Where(p => p.Status == ProviderStatus.Published);
        if (query.Type is not null)
        {
            providerQueryable = providerQueryable.Where(p => p.Type == query.Type);
        }
        return await providerQueryable.OrderBy(p => p.DisplayName).ThenBy(p => p.Id).Select(ToProviderSummaryDto).PaginateAsync(query);
    }

    // POST /api/providers
    public async Task<ProviderDto> CreateAsync(ProviderSaveDto providerSaveDto)
    {
        ProviderSaveDtoNormalizer.Normalize(providerSaveDto);

        var offeringIds = providerSaveDto.OfferingIds.Distinct().ToList();
        await EnsureOfferingsExist(offeringIds);
        await EnsureInstagramIsFree(providerSaveDto.InstagramUsername);

        var now = DateTime.UtcNow;
        var provider = new Provider
        {
            DisplayName = string.Empty,
            PhoneNumber = string.Empty,
            Status = ProviderStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now,
            Offerings = offeringIds.Select(id => new ProviderOffering { OfferingId = id, CreatedAt = now }).ToList()
        };
        ApplyFields(provider, providerSaveDto);

        // Se guarda junto con el Provider: un solo SaveChanges, una sola transacción.
        provider.Consents.Add(new ProviderConsent
        {
            PhoneHash = phoneHasher.Hash(provider.PhoneNumber),
            AcceptedAt = now
        });

        dbContext.Add(provider);
        await SaveChangesCatchingDuplicateInstagram();

        return await LoadByIdAsync(provider.Id);
    }

    // PUT /api/providers/{id}
    public async Task UpdateAsync(int id, ProviderSaveDto providerSaveDto)
    {
        ProviderSaveDtoNormalizer.Normalize(providerSaveDto);

        var provider = await dbContext.Providers
                           .Include(p => p.Offerings)
                           .FirstOrDefaultAsync(p => p.Id == id)
                       ?? throw new NotFoundException("provider", id);

        var offeringIds = providerSaveDto.OfferingIds.Distinct().ToList();
        await EnsureOfferingsExist(offeringIds);
        await EnsureInstagramIsFree(providerSaveDto.InstagramUsername, excludeId: id);

        ApplyFields(provider, providerSaveDto);
        provider.UpdatedAt = DateTime.UtcNow;

        provider.Offerings.RemoveAll(po => !offeringIds.Contains(po.OfferingId));
        var currentIds = provider.Offerings.Select(po => po.OfferingId).ToList();
        provider.Offerings.AddRange(offeringIds
            .Except(currentIds)
            .Select(offeringId => new ProviderOffering { OfferingId = offeringId, CreatedAt = DateTime.UtcNow }));

        await SaveChangesCatchingDuplicateInstagram();
    }

    // DELETE /api/providers/{id}
    public async Task DeleteAsync(int id)
    {
        var deletedRows = await dbContext.Providers
            .Where(p => p.Id == id)
            .ExecuteDeleteAsync();

        if (deletedRows == 0)
        {
            throw new NotFoundException("provider", id);
        }
    }

    // GET /api/providers/{id}: solo proveedores publicados.
    public async Task<ProviderDto> GetByIdAsync(int id)
    {
        return await dbContext.Providers
                   .Where(p => p.Id == id && p.Status == ProviderStatus.Published)
                   .Select(ToProviderDto)
                   .FirstOrDefaultAsync()
               ?? throw new NotFoundException("provider", id);
    }

    // Sin filtrar por estado: para devolver el proveedor recién creado (queda Pending).
    private async Task<ProviderDto> LoadByIdAsync(int id)
    {
        return await dbContext.Providers
                   .Where(p => p.Id == id)
                   .Select(ToProviderDto)
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

        IQueryable<Provider> providersDb = dbContext.Providers
            .Where(p => p.Status == ProviderStatus.Published)
            .Where(p => p.Offerings.Any(po => po.OfferingId == offeringId));

        return await providersDb
            .OrderBy(p => p.DisplayName)
            .ThenBy(p => p.Id)
            .Select(ToProviderSummaryDto)
            .PaginateAsync(pageQuery);
    }

    // Proveedores publicados cuyo nombre, o el de alguno de sus servicios, contiene el texto
    // (sin importar tildes ni mayúsculas).
    public async Task<PagedResult<ProviderSummaryDto>> SearchByNameOrServiceNameAsync(string text, PageQuery pageQuery)
    {
        var pattern = LikePattern.Contains(text.Trim());

        return await dbContext.Providers
            .Where(p => p.Status == ProviderStatus.Published)
            .Where(p => EF.Functions.Like(AppDbContext.FUnaccent(p.DisplayName.ToLower()), AppDbContext.FUnaccent(pattern))
                        || p.Offerings.Any(po => EF.Functions.Like(
                            AppDbContext.FUnaccent(po.Offering.Name.ToLower()), AppDbContext.FUnaccent(pattern))))
            .OrderBy(p => p.DisplayName)
            .ThenBy(p => p.Id)
            .Select(ToProviderSummaryDto)
            .PaginateAsync(pageQuery);
    }

    // GET /api/search: página de proveedores + filtros (categorías y servicios).
    // Paso 1: solo la página de proveedores; los filtros vuelven vacíos.
    public async Task<SearchResponseDto> SearchAsync(SearchQuery query)
    {
        var providers = await SearchProvidersPageAsync(query);

        // Paso 2 (después): una consulta después de la otra, el DbContext no admite dos a la vez.
        // var categories = await GetCategoryFiltersAsync(query);
        // var offerings = await GetOfferingFiltersAsync(query);

        return new SearchResponseDto
        {
            Providers = providers,
            Filters = new SearchFiltersDto()
        };
    }

    // Publicados + filtros de la request. Cada filtro que viene suma un Where ("y").
    // Para texto: SearchText.Matches(columna, LikePattern.Contains(texto)).
    private IQueryable<Provider> FilterForSearch(SearchQuery query)
    {
        var providers = GetPublishedProviders();

        if (query.ProviderType is not null)
        {
            providers = providers.Where(p => p.Type == query.ProviderType);
        }

        if (!string.IsNullOrWhiteSpace(query.Text))
        {
            var pattern = LikePattern.Contains(query.Text.Trim());
            // busco si el text coincide con nombre, descripcion del provider o con el nombre de alguno de los serivicos qeu brinda
            providers = providers.Where(p =>
                SearchText.Matches(p.DisplayName, pattern)
                || SearchText.Matches(p.Description, pattern)
                || p.Offerings.Any(po => SearchText.Matches(po.Offering.Name, pattern)));
        }

        //Busco providers que tengan algun servicio que pertenezca a alguna de las categorias que viene por query
        if (query.CategoryIds.Count > 0)
        {
            providers = providers
                .Where(p => p.Offerings.Any(po => query.CategoryIds.Contains(po.Offering.CategoryId)));
        }

        if (query.OfferingId is not null)
        {
            providers = providers.Where(p => p.Offerings.Any(po => po.OfferingId == query.OfferingId));
        }
        return providers;
    }

    // Featured: perfil más completo primero (foto, descripción, Instagram), después el más nuevo.
    // Newest: FirstPublishedAt descendente. Los dos desempatan por Id para que las páginas no se pisen.
    private static IQueryable<Provider> SortForSearch(IQueryable<Provider> providers, ProviderSort sortBy)
    {
        throw new NotImplementedException();
    }

    // providers: Total con FilterForSearch(query); Items ordenados con SortForSearch y paginados;
    // HasMore = Page * PageSize < Total.
    private async Task<ProviderSearchPageDto> SearchProvidersPageAsync(SearchQuery query)
    {
        var providers = FilterForSearch(query);

        // Total de todos los que cumplen los filtros, antes de paginar.
        var total = await providers.CountAsync();

        // Orden provisorio por Id hasta que esté SortForSearch. El orden va antes del Select.
        var page = await providers
            .OrderBy(p => p.Id)
            .Select(ToProviderSummaryDto)
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

    // Paso 2. filters.categories: cuántos proveedores tiene cada categoría, con los filtros de la request
    // menos las categorías elegidas. Solo las que tienen al menos uno, ordenadas por nombre.
    private Task<List<CategoryFilterDto>> GetCategoryFiltersAsync(SearchQuery query)
    {
        throw new NotImplementedException();
    }

    // Paso 2. filters.offerings: servicios cuyo nombre coincide con el texto, con cuántos proveedores
    // los ofrecen (filtros de la request menos el servicio elegido). Solo los que tienen alguno, hasta
    // OfferingFiltersLimit, por cantidad y nombre. Sin texto, lista vacía.
    private Task<List<OfferingFilterDto>> GetOfferingFiltersAsync(SearchQuery query)
    {
        throw new NotImplementedException();
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
            .Select(ToProviderSummaryDto)
            .ToListAsync();
        return providersDb;
    }


    public async Task<List<ProviderSummaryDto>> GetFeaturedAsync()
    {
        var providersDb = await GetPublishedProviders()
            .Where(p => p.Description != null && p.InstagramUsername != null  && p.PhotoKey != null)
            .OrderBy(p => p.DisplayName)
            .ThenBy(p => p.Id)
            .Take(6)
            .Select(ToProviderSummaryDto)
            .ToListAsync();
        return providersDb;
    }



    // Copia los datos del request (ya normalizado y validado) al Provider.
    private static void ApplyFields(Provider provider, ProviderSaveDto dto)
    {
        var type = dto.Type!.Value;
        var isIndividual = type == ProviderType.Individual;

        provider.Type = type;
        provider.FirstName = isIndividual ? dto.FirstName : null;
        provider.LastName = isIndividual ? dto.LastName : null;
        provider.DisplayName = isIndividual ? $"{dto.FirstName} {dto.LastName}" : dto.DisplayName!;
        provider.Description = dto.Description;
        provider.PhotoKey = dto.PhotoKey;
        provider.PhoneNumber = dto.PhoneNumber!;
        provider.InstagramUsername = dto.InstagramUsername;
        provider.Address = dto.Address;
        provider.Hours = dto.Hours;
    }

    private async Task EnsureInstagramIsFree(string? instagramUsername, int? excludeId = null)
    {
        if (instagramUsername is null)
        {
            return;
        }

        if (await dbContext.Providers.AnyAsync(p => p.Id != excludeId && p.InstagramUsername == instagramUsername))
        {
            throw DuplicateInstagram();
        }
    }

    // Si dos requests con el mismo Instagram pasan la consulta a la vez, el índice único ataja al segundo.
    private async Task SaveChangesCatchingDuplicateInstagram()
    {
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
                                           {
                                               SqlState: PostgresErrorCodes.UniqueViolation,
                                               ConstraintName: InstagramUniqueIndex
                                           })
        {
            throw DuplicateInstagram();
        }
    }

    private static ConflictException DuplicateInstagram()
        => new("duplicate_instagram", "Ya hay un proveedor con ese usuario de Instagram.", "instagramUsername");

    private async Task EnsureOfferingsExist(List<int> offeringIds)
    {
        var existingIds = await dbContext.Offerings
            .Where(o => offeringIds.Contains(o.Id))
            .Select(o => o.Id)
            .ToListAsync();

        var missing = offeringIds.Except(existingIds).ToList();
        if (missing.Count > 0)
        {
            throw ValidationFailedException.ForField("offeringIds", $"No existen los servicios: {string.Join(", ", missing)}.");
        }
    }
}
