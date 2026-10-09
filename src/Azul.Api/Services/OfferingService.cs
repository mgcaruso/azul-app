using System.Linq.Expressions;
using Azul.Api.Common.Exceptions;
using Azul.Api.Common.Pagination;
using Azul.Api.Data;
using Azul.Api.DTOs;
using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Azul.Api.Services;

public class OfferingService(AppDbContext dbContext) : IOfferingService
{
    private static readonly Expression<Func<Offering, OfferingDto>> ToDto =
        o => new OfferingDto { Id = o.Id, Name = o.Name, CategoryId = o.CategoryId };

    private static readonly Expression<Func<Offering, OfferingSummaryDto>> ToSummaryDto =
        o => new OfferingSummaryDto { Id = o.Id, Name = o.Name };

    public async Task<PagedResult<OfferingSummaryDto>> SearchAsync(OfferingSearchQuery query)
    {
        IQueryable<Offering> offerings = dbContext.Offerings;

        if (query.CategoryId is not null)
        {
            var categoryId = query.CategoryId.Value;
            var categoryExists = await dbContext.Categories.AnyAsync(c => c.Id == categoryId);
            if (!categoryExists)
            {
                throw new NotFoundException("category", categoryId);
            }

            offerings = offerings.Where(o => o.CategoryId == categoryId);
        }

        var text = query.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(text))
        {
            var pattern = $"%{EscapeLike(text)}%".ToLowerInvariant();
            offerings = offerings.Where(o =>
                EF.Functions.Like(AppDbContext.FUnaccent(o.Name.ToLower()), AppDbContext.FUnaccent(pattern)));
        }

        return await offerings
            .OrderBy(o => o.Name)
            .ThenBy(o => o.Id)
            .Select(ToSummaryDto)
            .PaginateAsync(query);
    }

    public async Task<OfferingDto> GetByIdAsync(int id)
    {
        return await dbContext.Offerings
                   .Where(o => o.Id == id)
                   .Select(ToDto)
                   .FirstOrDefaultAsync()
               ?? throw new NotFoundException("offering", id);
    }

    public async Task<List<OfferingSummaryDto>> GetByCategoryAsync(int categoryId)
    {
        Console.WriteLine("categoria ", categoryId);
        var categoryExists = await dbContext.Categories.AnyAsync(c => c.Id == categoryId);
        if (!categoryExists)
        {
            throw new NotFoundException("category", categoryId);
        }

        return await dbContext.Offerings
            .Where(o => o.CategoryId == categoryId)
            .OrderBy(o => o.Name)
            .Select(ToSummaryDto)
            .ToListAsync();
    }

    public async Task<OfferingDto> CreateAsync(OfferingSaveDto offeringSaveDto)
    {
        var categoryId = offeringSaveDto.CategoryId!.Value;
        if (!await dbContext.Categories.AnyAsync(c => c.Id == categoryId))
        {
            throw ValidationFailedException.ForField("categoryId", "La categoría no existe.");
        }

        var name = offeringSaveDto.Name.Trim();
        if (await NameExists(name, categoryId))
        {
            throw ConflictException.Duplicate("name");
        }

        var offering = new Offering
        {
            Name = name,
            CategoryId = categoryId
        };
        dbContext.Add(offering);
        await dbContext.SaveChangesAsync();

        return new OfferingDto
        {
            Id = offering.Id,
            Name = offering.Name,
            CategoryId = offering.CategoryId
        };
    }

    public async Task UpdateAsync(int id, OfferingSaveDto offeringSaveDto)
    {
        var offering = await dbContext.Offerings.FindAsync(id)
                       ?? throw new NotFoundException("offering", id);

        var categoryId = offeringSaveDto.CategoryId!.Value;
        if (categoryId != offering.CategoryId
            && !await dbContext.Categories.AnyAsync(c => c.Id == categoryId))
        {
            throw ValidationFailedException.ForField("categoryId", "La categoría no existe.");
        }

        var name = offeringSaveDto.Name.Trim();
        if (await NameExists(name, categoryId, excludeId: id))
        {
            throw ConflictException.Duplicate("name");
        }

        offering.Name = name;
        offering.CategoryId = categoryId;

        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var deletedRows = await dbContext.Offerings
            .Where(o => o.Id == id)
            .ExecuteDeleteAsync();

        if (deletedRows == 0)
        {
            throw new NotFoundException("offering", id);
        }
    }

    private static string EscapeLike(string value)
        => value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    private Task<bool> NameExists(string name, int categoryId, int? excludeId = null)
    {
        return dbContext.Offerings
            .AnyAsync(o => o.Id != excludeId
                           && o.CategoryId == categoryId
                           && o.Name.ToLower() == name.ToLower());
    }
}
