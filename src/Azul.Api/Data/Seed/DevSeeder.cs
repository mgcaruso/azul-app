using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Azul.Api.Data.Seed;

// Carga categorías y servicios. Los proveedores se cargan a mano desde la API.
public static class DevSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (await dbContext.Categories.AnyAsync())
        {
            return;
        }

        var categories = CatalogSeedData.Catalog.Keys
            .Select(name => new Category { Name = name })
            .ToList();
        dbContext.AddRange(categories);
        await dbContext.SaveChangesAsync();

        var offerings = categories
            .SelectMany(category => CatalogSeedData.Catalog[category.Name]
                .Select(name => new Offering { Name = name, CategoryId = category.Id }))
            .ToList();
        dbContext.AddRange(offerings);
        await dbContext.SaveChangesAsync();
    }
}
