using System.Text.Json;
using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Azul.Api.Data.Seed;

// Loads the launch catalog (categories, offerings and their keywords) from the embedded catalog.json.
// It only runs on a database without categories, so admin changes are never overwritten.
public static class CatalogSeeder
{
    private const string ResourceName = "catalog.json";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (await dbContext.Categories.AnyAsync())
        {
            return;
        }

        var catalog = await ReadCatalogAsync();

        // One transaction: either the whole catalog is loaded or nothing is, so a failed start can retry.
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var categories = catalog.Categories
            .Select(c => new Category { Name = c.Name })
            .ToList();
        dbContext.AddRange(categories);
        await dbContext.SaveChangesAsync();

        var offerings = catalog.Categories
            .Zip(categories)
            .SelectMany(pair => pair.First.Offerings.Select(o => new Offering
            {
                Name = o.Name,
                Keywords = o.Keywords,
                CategoryId = pair.Second.Id
            }))
            .ToList();
        dbContext.AddRange(offerings);
        await dbContext.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    private static async Task<CatalogFile> ReadCatalogAsync()
    {
        await using var stream = typeof(CatalogSeeder).Assembly.GetManifestResourceStream(ResourceName)
                                 ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' was not found.");

        return await JsonSerializer.DeserializeAsync<CatalogFile>(stream, JsonOptions)
               ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' is empty.");
    }

    private sealed class CatalogFile
    {
        public List<CatalogCategory> Categories { get; set; } = [];
    }

    private sealed class CatalogCategory
    {
        public string Name { get; set; } = string.Empty;
        public List<CatalogOffering> Offerings { get; set; } = [];
    }

    private sealed class CatalogOffering
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Keywords { get; set; } = [];
    }
}
