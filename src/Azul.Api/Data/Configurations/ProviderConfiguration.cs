using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Azul.Api.Data.Configurations;

public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> p)
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

        p.HasIndex(x => x.ClientId)
            .IsUnique()
            .HasDatabaseName("IX_Providers_ClientId");

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
                "Status" IN ('Pending', 'Published', 'Suspended', 'Rejected')
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
    }
}
