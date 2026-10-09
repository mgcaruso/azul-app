using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Azul.Api.Data.Seed;

// Llena la base con datos de prueba para medir cómo responden SQL y la API.
// Corre al arrancar la API, solo en Development y solo si la base está vacía (ver Program.cs).
public static class DevSeeder
{
    // Semilla fija: cada vez que vacíes la base y vuelvas a arrancar, se generan los mismos datos.
    // Así podés comparar mediciones de antes y después de un índice.
    private const int RandomSeed = 42;

    private const int ProviderCount = 5_000;

    public static async Task SeedAsync(AppDbContext dbContext)
    {
        // TODO (Guada):
        // 1. Si ya hay categorías, no hacer nada (AnyAsync). Así no se duplica nada en cada arranque.
        // 2. Catálogo: recorrer CatalogSeedData.Catalog, crear cada Category con sus Offering
        //    y un AddRange + SaveChangesAsync. Después del SaveChanges, EF ya completó los Id.
        // 3. Proveedores: new Random(RandomSeed) y generar ProviderCount con GenerateProvider.
        //    AddRange + SaveChangesAsync (si tarda mucho, probá guardar de a lotes de 1.000).
        throw new NotImplementedException();
    }

    // Un proveedor con nombre inventado y de 1 a 3 servicios distintos elegidos al azar.
    private static Provider GenerateProvider(Random random, List<int> offeringIds)
    {
        // TODO (Guada):
        // - Nombre y tipo: elegir uno de CatalogSeedData.NameFormats. El nombre sale de
        //   string.Format(formato.Format, nombre, apellido) y el tipo es formato.Type.
        // - Servicios: random.Next(1, 4) da 1, 2 o 3. Que no se repitan (si no, choca la clave compuesta).
        // - Devolver el Provider con Offerings = [new ProviderOffering { OfferingId = ... }, ...].
        throw new NotImplementedException();
    }
}
