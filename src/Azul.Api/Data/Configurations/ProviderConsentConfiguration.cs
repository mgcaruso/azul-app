using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Azul.Api.Data.Configurations;

public class ProviderConsentConfiguration : IEntityTypeConfiguration<ProviderConsent>
{
    public void Configure(EntityTypeBuilder<ProviderConsent> pc)
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
    }
}
