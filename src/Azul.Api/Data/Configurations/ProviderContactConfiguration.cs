using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Azul.Api.Data.Configurations;

public class ProviderContactConfiguration : IEntityTypeConfiguration<ProviderContact>
{
    public void Configure(EntityTypeBuilder<ProviderContact> c)
    {
        // La clave es la propia ficha: una fila de contacto por ficha. Si se borra la ficha, se borra el contacto.
        c.HasKey(x => x.ProviderId);

        c.HasOne(x => x.Provider)
            .WithOne(p => p.Contact)
            .HasForeignKey<ProviderContact>(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Único. Los nulos no chocan entre sí en Postgres.
        c.HasIndex(x => x.InstagramUsername)
            .IsUnique()
            .HasDatabaseName("IX_ProviderContacts_InstagramUsername");

        // Para contar fichas por teléfono y detectar duplicados.
        c.HasIndex(x => x.PhoneNumber);
        c.HasIndex(x => x.CallPhone);

        c.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_ProviderContacts_PhoneNumber",
                """
                "PhoneNumber" IS NULL OR ("PhoneNumber" ~ '^\+[1-9]\d{7,14}$' AND ("PhoneNumber" !~ '^\+54' OR "PhoneNumber" ~ '^\+549\d{10}$'))
                """);

            // Los dos teléfonos se guardan en formato internacional (E.164): + y entre 8 y 15 dígitos. Así puede cargarse
            // también un número extranjero. Para Argentina el control es más estricto: el WhatsApp debe ser +549 y 10 dígitos, y el fijo +54 y 10 dígitos que no empiecen con 9.
            // El service no corrige números: solo quita espacios y guiones; la guía para escribirlos bien está en el formulario.
            t.HasCheckConstraint(
                "CK_ProviderContacts_CallPhone",
                """
                "CallPhone" IS NULL OR ("CallPhone" ~ '^\+[1-9]\d{7,14}$' AND ("CallPhone" !~ '^\+54' OR "CallPhone" ~ '^\+54[1-8]\d{9}$'))
                """);

            t.HasCheckConstraint(
                "CK_ProviderContacts_AtLeastOnePhone",
                """
                "PhoneNumber" IS NOT NULL OR "CallPhone" IS NOT NULL
                """);

            t.HasCheckConstraint(
                "CK_ProviderContacts_InstagramUsername_Lower",
                """
                "InstagramUsername" IS NULL OR "InstagramUsername" = lower("InstagramUsername")
                """);
        });
    }
}
