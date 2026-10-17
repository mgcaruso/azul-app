using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Azul.Api.Data.Configurations;

public class ProviderPhotoConfiguration : IEntityTypeConfiguration<ProviderPhoto>
{
    public void Configure(EntityTypeBuilder<ProviderPhoto> pp)
    {
        pp.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        pp.HasOne(x => x.Provider)
            .WithMany(p => p.Photos)
            .HasForeignKey(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);

        pp.HasIndex(x => x.ProviderId)
            .IsUnique()
            .HasFilter("""
                       "Status" = 'Pending'
                       """);

        pp.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_ProviderPhotos_Status",
                """
                "Status" IN ('Pending', 'Approved', 'Rejected')
                """);

            t.HasCheckConstraint(
                "CK_ProviderPhotos_ReviewedAt",
                """
                "ReviewedAt" IS NULL OR "ReviewedAt" >= "CreatedAt"
                """);
        });
    }
}
