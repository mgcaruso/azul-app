using System.Linq.Expressions;
using Azul.Api.Common.Exceptions;
using Azul.Api.Common.Pagination;
using Azul.Api.Data;
using Azul.Api.DTOs;
using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Azul.Api.Services;

public class ProviderService(AppDbContext dbContext) : IProviderService
{
    
    private static readonly Expression<Func<Provider, ProviderSummaryDto>> ToProviderSummaryDto =
        p => new ProviderSummaryDto { Id = p.Id, Name = p.Name, Type = p.Type };

    private static readonly Expression<Func<Provider, ProviderDto>> ToProviderDto =
        p => new ProviderDto
        {
            Id = p.Id,
            Name = p.Name,
            Type = p.Type,
            Offerings = p.Offerings
                .OrderBy(po => po.Offering.Name)
                .Select(po => new OfferingSummaryDto { Id = po.Offering.Id, Name = po.Offering.Name })
                .ToList()
        };

    // GET /api/providers?type=&page=&pageSize=
    public async Task<PagedResult<ProviderSummaryDto>> GetAllAsync(ProviderSearchQuery query)
    
    {
        
        IQueryable<Provider> providerQueryable = dbContext.Providers;
        if (query.Type is not null)
        {
            providerQueryable = providerQueryable.Where(p => p.Type == query.Type);
        }
        return await providerQueryable.OrderBy( p => p.Name).ThenBy(p => p.Id).Select(ToProviderSummaryDto).PaginateAsync(query);
    }

    // POST /api/providers
    public async Task<ProviderDto> CreateAsync(ProviderSaveDto providerSaveDto)
    {
        var offeringIds = providerSaveDto.OfferingIds.Distinct().ToList();
        
        var servicesDb =await dbContext.Offerings.Where(p => offeringIds.Contains(p.Id)).ToListAsync();
       
        if (servicesDb.Count != offeringIds.Count)
        {
            var missing = offeringIds.Except(servicesDb.Select(o => o.Id));
            throw ValidationFailedException.ForField("offeringIds", $"No existen los servicios: {string.Join(", ", missing)}.");
        }

        var provider = new Provider
        {
            Name = providerSaveDto.Name.Trim(),
            Type = providerSaveDto.Type!.Value,
            Offerings = offeringIds.Select(id => new ProviderOffering { OfferingId = id, CreatedAt = DateTime.UtcNow }).ToList()
        };
        dbContext.Add(provider);

        await dbContext.SaveChangesAsync();

        return await GetByIdAsync(provider.Id);
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

    // GET /api/services/{offeringId}/providers?page=&pageSize=
    public async Task<PagedResult<ProviderSummaryDto>> GetByOfferingAsync(int offeringId, PageQuery pageQuery)
    {
        
        var offeringDb =await  dbContext.Offerings.AnyAsync(o => o.Id == offeringId);
        if (!offeringDb)
        {
            throw new NotFoundException("offering", offeringId);
        }

        IQueryable<Provider> providersDb = dbContext.Providers.Where( p => p.Offerings.Any(po => po.Offering.Id == offeringId)); 
        
        return await providersDb
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Select(ToProviderSummaryDto)
            .PaginateAsync(pageQuery);
    }
    
}
