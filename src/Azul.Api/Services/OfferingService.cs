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

    // Búsqueda y listado: GET /api/services?text=&categoryId=&page=&pageSize=
    // Los filtros son opcionales y se combinan (AND). Sin ninguno, devuelve todos los servicios paginados.
    public async Task<PagedResult<OfferingSummaryDto>> SearchAsync(OfferingSearchQuery query)
    {
        // Mientras sea IQueryable no se ejecuta nada: cada Where suma una condición al SQL,
        // que recién se manda a la base en PaginateAsync.
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
            // Se escapan \, % y _ para que el usuario los busque como texto y no como comodines.
            // Ignora mayúsculas y tildes: lower() + f_unaccent() de los dos lados, con la misma
            // expresión que el índice trigram (IX_Offerings_Name_Search) para que Postgres lo use.
            var pattern = $"%{EscapeLike(text)}%".ToLowerInvariant();
            offerings = offerings.Where(o =>
                EF.Functions.Like(AppDbContext.FUnaccent(o.Name.ToLower()), AppDbContext.FUnaccent(pattern)));
        }

        // ThenBy(Id) desempata: el nombre se puede repetir entre categorías y sin un orden
        // único la misma fila podría aparecer en dos páginas.
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

    // Servicios de una categoría: GET /api/categories/{categoryId}/services
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

    // En LIKE, % y _ son comodines y \ es el carácter de escape de Postgres.
    private static string EscapeLike(string value)
        => value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

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
