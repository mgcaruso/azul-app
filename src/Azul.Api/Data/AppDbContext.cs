using Azul.Api.Common.Search;
using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Azul.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Offering> Offerings => Set<Offering>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<ProviderOffering> ProviderOfferings => Set<ProviderOffering>();
    public DbSet<ProviderConsent> ProviderConsents => Set<ProviderConsent>();
    public DbSet<ProviderPhoto> ProviderPhotos => Set<ProviderPhoto>();
    
    public static string FUnaccent(string text) => throw new NotSupportedException();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("unaccent");
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.HasDbFunction(() => FUnaccent(default!)).HasName("f_unaccent");
        modelBuilder.HasDbFunction(() => SearchText.Matches(default, default!)).HasName("f_matches");

        // La configuración de cada entidad está en Data/Configurations.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
