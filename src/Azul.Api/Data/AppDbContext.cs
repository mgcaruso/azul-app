using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Azul.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Offering> Offerings => Set<Offering>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<ProviderOffering> ProviderOfferings => Set<ProviderOffering>();
    
    // Para usarla en LINQ. Nunca se ejecuta en C#: EF la traduce a f_unaccent(...) en SQL.
    public static string FUnaccent(string text) => throw new NotSupportedException();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("unaccent");
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.HasDbFunction(() => FUnaccent(default!)).HasName("f_unaccent");

        modelBuilder.Entity<Provider>(p =>
        {
            // El enum se guarda como texto ("Individual", "Business") en vez de 0 y 1.
            p.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Y la base solo acepta esos dos valores, aunque alguien inserte por SQL a mano.
            // Si sumás un valor al enum, hay que actualizar esta lista y generar una migración.
            p.ToTable(t => t.HasCheckConstraint(
                "CK_Providers_Type",
                """
                "Type" IN ('Individual', 'Business')
                """));
        });

        modelBuilder.Entity<ProviderOffering>(po =>
        {
            // Clave compuesta. Como empieza por ProviderId, también sirve de índice
            // para "los servicios de un proveedor".
            po.HasKey(x => new { x.ProviderId, x.OfferingId });

            // Si se borra un proveedor, se borran sus filas acá (baja real, decisión D6).
            po.HasOne(x => x.Provider)
                .WithMany(p => p.Offerings)
                .HasForeignKey(x => x.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Un servicio que algún proveedor ofrece NO se puede borrar: la base lo rechaza.
            // EF crea solo el índice sobre OfferingId, que es el que usa "proveedores de un servicio".
            po.HasOne(x => x.Offering)
                .WithMany()
                .HasForeignKey(x => x.OfferingId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
