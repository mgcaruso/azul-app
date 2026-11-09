using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Azul.Api.Data.Seed;

// Loads a small set of hand-made providers for local development. Each one covers a case worth testing
// (single category, mixed categories, tie, no offerings, every status). It only runs on a database
// without providers and only in Development; the offerings must already exist (see CatalogSeeder).
public static class DevProviderSeeder
{
    private sealed record SeedProvider(
        ProviderType Type,
        string Name,
        ProviderStatus Status,
        string[] Offerings,
        string? Whatsapp = null,
        string? LandLine = null,
        string? Instagram = null,
        string? Address = null,
        string? Hours = null,
        string? Description = null);

    private static readonly SeedProvider[] Providers =
    [
        // One category only: MainCategory = Hogar y reparaciones.
        new(ProviderType.Individual, "Juan Pérez", ProviderStatus.Published,
            ["Plomería", "Gasista"], Whatsapp: "+5492281000001", Instagram: "juan.plomero",
            Description: "Plomero y gasista matriculado. Urgencias las 24 horas."),

        // Mixed categories, one wins by count: 2 Hogar vs 1 Educación -> Hogar y reparaciones.
        new(ProviderType.Individual, "Marta Gómez", ProviderStatus.Published,
            ["Electricista", "Plomería", "Idiomas"], Whatsapp: "+5492281000002",
            Description: "Instalaciones eléctricas y clases de inglés."),

        // Tie, 1 offering each: the category name decides -> Arte y cultura.
        new(ProviderType.Venture, "Movimiento Azul", ProviderStatus.Published,
            ["Pilates", "Danza"], Whatsapp: "+5492281000003", Instagram: "movimiento.azul",
            Description: "Pilates y danza para todas las edades."),

        // No offerings: MainCategory must be null.
        new(ProviderType.Venture, "Sin Servicios Todavía", ProviderStatus.Published,
            [], Whatsapp: "+5492281000004"),

        // Business with address, hours and landline.
        new(ProviderType.Business, "Veterinaria San Roque", ProviderStatus.Published,
            ["Veterinaria", "Peluquería canina"], LandLine: "+542281430001", Whatsapp: "+5492281000005",
            Instagram: "vet.sanroque", Address: "Av. Mitre 1200", Hours: "Lunes a sábados de 9 a 20",
            Description: "Consultas, vacunas y peluquería canina."),

        new(ProviderType.Business, "Mecánica del Centro", ProviderStatus.Published,
            ["Mecánica", "Gomería", "Electricidad del automotor"], LandLine: "+542281430002",
            Address: "San Martín 850", Hours: "Lunes a viernes de 8 a 18"),

        new(ProviderType.Venture, "Dulce Tentación", ProviderStatus.Published,
            ["Tortas y repostería", "Viandas y comida casera"], Whatsapp: "+5492281000006",
            Instagram: "dulce.tentacion.azul", Description: "Tortas por encargo y viandas caseras."),

        new(ProviderType.Individual, "Lucía Fernández", ProviderStatus.Published,
            ["Peluquería y barbería", "Maquillaje"], Whatsapp: "+5492281000007", Instagram: "lucia.estilista"),

        new(ProviderType.Individual, "Diego Torres", ProviderStatus.Published,
            ["Reparación de celulares", "Reparación de notebooks y PC"], Whatsapp: "+5492281000008"),

        new(ProviderType.Venture, "Fiestas del Sur", ProviderStatus.Published,
            ["DJ y animación", "Sonido e iluminación", "Decoración de eventos"], Whatsapp: "+5492281000009",
            Instagram: "fiestas.delsur"),

        new(ProviderType.Individual, "Camila Romero", ProviderStatus.Published,
            ["Psicología"], Whatsapp: "+5492281000010"),

        // Not public: one per status, to check they never show up in public queries.
        new(ProviderType.Individual, "Pedro Pendiente", ProviderStatus.Pending,
            ["Pintura"], Whatsapp: "+5492281000011"),

        new(ProviderType.Individual, "Susana Suspendida", ProviderStatus.Suspended,
            ["Limpieza de hogares y oficinas"], Whatsapp: "+5492281000012"),

        new(ProviderType.Individual, "Rodolfo Rechazado", ProviderStatus.Rejected,
            ["Fletes y mudanzas"], Whatsapp: "+5492281000013")
    ];

    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (await dbContext.Providers.AnyAsync())
        {
            return;
        }

        var offeringIdsByName = await dbContext.Offerings
            .ToDictionaryAsync(o => o.Name, o => o.Id);

        var now = DateTime.UtcNow;
        var providers = Providers.Select(seed => ToProvider(seed, offeringIdsByName, now)).ToList();

        dbContext.AddRange(providers);
        await dbContext.SaveChangesAsync();
    }

    private static Provider ToProvider(SeedProvider seed, Dictionary<string, int> offeringIdsByName, DateTime now)
    {
        var isIndividual = seed.Type == ProviderType.Individual;
        var nameParts = seed.Name.Split(' ', 2);

        return new Provider
        {
            Type = seed.Type,
            DisplayName = seed.Name,
            FirstName = isIndividual ? nameParts[0] : null,
            LastName = isIndividual ? nameParts[1] : null,
            Description = seed.Description,
            Address = seed.Address,
            Hours = seed.Hours,
            Status = seed.Status,
            FirstPublishedAt = seed.Status == ProviderStatus.Published ? now : null,
            CreatedAt = now,
            UpdatedAt = now,
            Contact = new ProviderContact
            {
                PhoneNumber = seed.Whatsapp,
                CallPhone = seed.LandLine,
                InstagramUsername = seed.Instagram
            },
            Offerings = seed.Offerings
                .Select(name => new ProviderOffering
                {
                    OfferingId = offeringIdsByName.TryGetValue(name, out var id)
                        ? id
                        : throw new InvalidOperationException($"Seed offering '{name}' is not in the catalog."),
                    CreatedAt = now
                })
                .ToList()
        };
    }
}
