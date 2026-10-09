using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Azul.Api.Data.Seed;

public static class DevSeeder
{
    private const int RandomSeed = 42;

    private const int ProviderCount = 5_000;

    public static async Task SeedAsync(AppDbContext dbContext)
    {
        throw new NotImplementedException();
    }

    private static Provider GenerateProvider(Random random, List<int> offeringIds)
    {
        throw new NotImplementedException();
    }
}
