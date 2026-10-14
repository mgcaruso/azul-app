using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Azul.Api.Data.Configurations;

public class ProviderOfferingConfiguration : IEntityTypeConfiguration<ProviderOffering>
{
    public void Configure(EntityTypeBuilder<ProviderOffering> po)
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
    }
}
