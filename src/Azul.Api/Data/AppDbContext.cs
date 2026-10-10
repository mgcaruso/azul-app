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

            p.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(ProviderStatus.Pending)
                .HasSentinel(ProviderStatus.Pending);

            p.HasIndex(x => x.InstagramUsername)
                .IsUnique()
                .HasDatabaseName("IX_Providers_InstagramUsername");

            p.HasIndex(x => x.PhoneNumber);

            p.HasIndex(x => x.FirstPublishedAt)
                .IsDescending()
                .HasFilter("""
                           "Status" = 'Published'
                           """);

            p.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_Providers_Type",
                    """
                    "Type" IN ('Individual', 'Venture', 'Business')
                    """);

                t.HasCheckConstraint(
                    "CK_Providers_Status",
                    """
                    "Status" IN ('Pending', 'Published', 'Suspended')
                    """);

                t.HasCheckConstraint(
                    "CK_Providers_DisplayName_NotBlank",
                    """
                    btrim("DisplayName") <> ''
                    """);

                t.HasCheckConstraint(
                    "CK_Providers_Name_OnlyIndividual",
                    """
                    "Type" = 'Individual' OR ("FirstName" IS NULL AND "LastName" IS NULL)
                    """);

                t.HasCheckConstraint(
                    "CK_Providers_Name_RequiredIndividual",
                    """
                    "Type" <> 'Individual' OR ("FirstName" IS NOT NULL AND "LastName" IS NOT NULL)
                    """);

                t.HasCheckConstraint(
                    "CK_Providers_Address_RequiredBusiness",
                    """
                    "Type" <> 'Business' OR "Address" IS NOT NULL
                    """);

                t.HasCheckConstraint(
                    "CK_Providers_PhoneNumber",
                    """
                    "PhoneNumber" ~ '^\+549\d{10}$'
                    """);

                t.HasCheckConstraint(
                    "CK_Providers_FirstPublishedAt",
                    """
                    "Status" <> 'Published' OR "FirstPublishedAt" IS NOT NULL
                    """);

                t.HasCheckConstraint(
                    "CK_Providers_InstagramUsername_Lower",
                    """
                    "InstagramUsername" IS NULL OR "InstagramUsername" = lower("InstagramUsername")
                    """);
            });
        });

        modelBuilder.Entity<ProviderConsent>(pc =>
        {
            pc.HasOne(x => x.Provider)
                .WithMany(p => p.Consents)
                .HasForeignKey(x => x.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);

            // Un solo consentimiento vigente por proveedor.
            pc.HasIndex(x => x.ProviderId)
                .IsUnique()
                .HasFilter("""
                           "RevokedAt" IS NULL
                           """);

            pc.ToTable(t => t.HasCheckConstraint(
                "CK_ProviderConsents_RevokedAt",
                """
                "RevokedAt" IS NULL OR "RevokedAt" >= "AcceptedAt"
                """));
        });

        modelBuilder.Entity<ProviderOffering>(po =>
        {
            po.HasKey(x => new { x.ProviderId, x.OfferingId });

            // La PK empieza por ProviderId; para buscar proveedores por servicio hace falta este.
            po.HasIndex(x => x.OfferingId);

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
