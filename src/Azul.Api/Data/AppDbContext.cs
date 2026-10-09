using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Azul.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Offering> Offerings => Set<Offering>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<ProviderOffering> ProviderOfferings => Set<ProviderOffering>();
    
    public static string FUnaccent(string text) => throw new NotSupportedException();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("unaccent");
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.HasDbFunction(() => FUnaccent(default!)).HasName("f_unaccent");

        modelBuilder.Entity<Provider>(p =>
        {
            p.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(20);

            p.ToTable(t => t.HasCheckConstraint(
                "CK_Providers_Type",
                """
                "Type" IN ('Individual', 'Business')
                """));
        });

        modelBuilder.Entity<ProviderOffering>(po =>
        {
            po.HasKey(x => new { x.ProviderId, x.OfferingId });

            po.HasOne(x => x.Provider)
                .WithMany(p => p.Offerings)
                .HasForeignKey(x => x.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);

            po.HasOne(x => x.Offering)
                .WithMany()
                .HasForeignKey(x => x.OfferingId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
