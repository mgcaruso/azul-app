using System.Linq.Expressions;
using Azul.Api.Common.Exceptions;
using Azul.Api.Data;
using Azul.Api.DTOs;
using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Azul.Api.Services;

public class OfferingService(AppDbContext dbContext) : IOfferingService
{
    private static readonly Expression<Func<Offering, OfferingDto>> ToDto =
        o => new OfferingDto { Id = o.Id, Name = o.Name, Description = o.Description, CategoryId = o.CategoryId };

    private static readonly Expression<Func<Offering, OfferingSummaryDto>> ToSummaryDto =
        o => new OfferingSummaryDto { Id = o.Id, Name = o.Name };

    // Búsqueda y listado: GET /api/services?text=&categoryId=&page=&pageSize=
    public async Task<OfferingSearchResultDto> SearchAsync(OfferingSearchQuery query)
    {
        // TODO (Guada):
        // 1. Si viene CategoryId y la categoría no existe -> ¿404 con new NotFoundException("category", categoryId) o lista vacía? (a decidir).
        // 2. Arrancar de dbContext.Offerings (IQueryable) y sumar los filtros solo si vienen:
        //    - CategoryId: Where por CategoryId.
        //    - Text (con Trim): EF.Functions.ILike(o.Name, $"%{texto}%"), escapando antes % y _ del texto
        //      para que se busquen literales (ILike no ignora tildes: eso viene con SearchText más adelante).
        // 3. OrderBy por Name.
        // 4. Paginar: Skip((Page - 1) * PageSize) y Take(PageSize + 1). Pedís uno de más:
        //    si vinieron más de PageSize, HasMore = true y te quedás con los primeros PageSize.
        // 5. Select(ToSummaryDto) y armar el OfferingSearchResultDto con Items, Page, PageSize y HasMore.
        throw new NotImplementedException();
    }

    public async Task<OfferingDto> GetByIdAsync(int id)
    {
        return await dbContext.Offerings
                   .Where(o => o.Id == id)
                   .Select(ToDto)
                   .FirstOrDefaultAsync()
               ?? throw new NotFoundException("offering", id);
    }

    // Servicios de una categoría: GET /api/categories/{categoryId}/services
    public async Task<List<OfferingSummaryDto>> GetByCategoryAsync(int categoryId)
    {
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
        // [Required] ya validó que vino, por eso el ! es seguro.
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
            Description = NormalizeDescription(offeringSaveDto.Description),
            CategoryId = categoryId
        };
        dbContext.Add(offering);
        await dbContext.SaveChangesAsync();

        return new OfferingDto
        {
            Id = offering.Id,
            Name = offering.Name,
            Description = offering.Description,
            CategoryId = offering.CategoryId
        };
    }

    public async Task UpdateAsync(int id, OfferingSaveDto offeringSaveDto)
    {
        var offering = await dbContext.Offerings.FindAsync(id)
                       ?? throw new NotFoundException("offering", id);

        // [Required] ya validó que vino, por eso el ! es seguro.
        var categoryId = offeringSaveDto.CategoryId!.Value;
        // Solo hace falta chequear la categoría si cambió: la actual existe sí o sí (FK).
        if (categoryId != offering.CategoryId
            && !await dbContext.Categories.AnyAsync(c => c.Id == categoryId))
        {
            throw ValidationFailedException.ForField("categoryId", "La categoría no existe.");
        }

        var name = offeringSaveDto.Name.Trim();
        // Contra la categoría de destino, sin contar este mismo servicio.
        if (await NameExists(name, categoryId, excludeId: id))
        {
            throw ConflictException.Duplicate("name");
        }

        offering.Name = name;
        offering.Description = NormalizeDescription(offeringSaveDto.Description);
        offering.CategoryId = categoryId;

        // Si nada cambió, EF no manda ningún UPDATE.
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        // TODO (iteración 2, con ProviderOffering): si algún proveedor ofrece este servicio,
        // tirar un 409 (offering.has_providers) en vez de borrarlo, y crear esa FK con
        // DeleteBehavior.Restrict desde el principio.
        var deletedRows = await dbContext.Offerings
            .Where(o => o.Id == id)
            .ExecuteDeleteAsync();

        if (deletedRows == 0)
        {
            throw new NotFoundException("offering", id);
        }
    }

    // "   " o "" se guardan como null: la descripción es opcional.
    private static string? NormalizeDescription(string? description)
        => string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    // ¿Hay otro servicio con este nombre en la misma categoría (sin distinguir mayúsculas)?
    // excludeId sirve en el Update, para no chocar con el servicio que se está editando.
    private Task<bool> NameExists(string name, int categoryId, int? excludeId = null)
    {
        return dbContext.Offerings
            .AnyAsync(o => o.Id != excludeId
                           && o.CategoryId == categoryId
                           && o.Name.ToLower() == name.ToLower());
    }
}
