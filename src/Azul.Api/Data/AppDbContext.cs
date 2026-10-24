using Azul.Api.Common.Search;
using Azul.Api.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Azul.Api.Data;

// Hereda de IdentityDbContext para sumar las tablas AspNet* (usuarios, logins externos, roles), todas con claves Guid.
// IDataProtectionKeyContext: guarda en la base las claves que cifran la cookie de sesión, para que sobrevivan a los reinicios de Cloud Run.
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options), IDataProtectionKeyContext
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Offering> Offerings => Set<Offering>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<ProviderOffering> ProviderOfferings => Set<ProviderOffering>();
    public DbSet<ProviderConsent> ProviderConsents => Set<ProviderConsent>();
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
    
    public static string FUnaccent(string text) => throw new NotSupportedException();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primero: acá Identity configura sus tablas (claves, índices, relaciones).
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("unaccent");
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.HasDbFunction(() => FUnaccent(default!)).HasName("f_unaccent");
        modelBuilder.HasDbFunction(() => SearchText.Matches(default, default!)).HasName("f_matches");

        // La configuración de cada entidad está en Data/Configurations.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
