using System.Linq.Expressions;
using Azul.Api.Common.Exceptions;
using Azul.Api.Common.Pagination;
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

        return await GetByIdAsync(provider.Id);
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

    // GET /api/providers/{id}
    public async Task<ProviderDto> GetByIdAsync(int id)
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
            .Where(p => p.Offerings.Any(po => po.Offering.Id == offeringId));

        return await providersDb
            .OrderBy(p => p.DisplayName)
            .ThenBy(p => p.Id)
            .Select(ToProviderSummaryDto)
            .PaginateAsync(pageQuery);
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
