using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Azul.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarProviderContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProviderContacts",
                columns: table => new
                {
                    ProviderId = table.Column<int>(type: "integer", nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CallPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    InstagramUsername = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ExtraUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderContacts", x => x.ProviderId);
                    table.CheckConstraint("CK_ProviderContacts_AtLeastOnePhone", "\"PhoneNumber\" IS NOT NULL OR \"CallPhone\" IS NOT NULL");
                    table.CheckConstraint("CK_ProviderContacts_CallPhone", "\"CallPhone\" IS NULL OR (\"CallPhone\" ~ '^\\+[1-9]\\d{7,14}$' AND (\"CallPhone\" !~ '^\\+54' OR \"CallPhone\" ~ '^\\+54[1-8]\\d{9}$'))");
                    table.CheckConstraint("CK_ProviderContacts_InstagramUsername_Lower", "\"InstagramUsername\" IS NULL OR \"InstagramUsername\" = lower(\"InstagramUsername\")");
                    table.CheckConstraint("CK_ProviderContacts_PhoneNumber", "\"PhoneNumber\" IS NULL OR (\"PhoneNumber\" ~ '^\\+[1-9]\\d{7,14}$' AND (\"PhoneNumber\" !~ '^\\+54' OR \"PhoneNumber\" ~ '^\\+549\\d{10}$'))");
                    table.ForeignKey(
                        name: "FK_ProviderContacts_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Pasa a la tabla nueva el celular (WhatsApp) y el Instagram que hoy están en Providers.
            migrationBuilder.Sql("""
                INSERT INTO "ProviderContacts" ("ProviderId", "PhoneNumber", "InstagramUsername")
                SELECT "Id", "PhoneNumber", "InstagramUsername" FROM "Providers";
                """);

            migrationBuilder.DropCheckConstraint(name: "CK_Providers_PhoneNumber", table: "Providers");
            migrationBuilder.DropCheckConstraint(name: "CK_Providers_InstagramUsername_Lower", table: "Providers");
            migrationBuilder.DropIndex(name: "IX_Providers_InstagramUsername", table: "Providers");
            migrationBuilder.DropIndex(name: "IX_Providers_PhoneNumber", table: "Providers");
            migrationBuilder.DropColumn(name: "InstagramUsername", table: "Providers");
            migrationBuilder.DropColumn(name: "PhoneNumber", table: "Providers");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderContacts_CallPhone",
                table: "ProviderContacts",
                column: "CallPhone");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderContacts_InstagramUsername",
                table: "ProviderContacts",
                column: "InstagramUsername",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderContacts_PhoneNumber",
                table: "ProviderContacts",
                column: "PhoneNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Antes del cambio el WhatsApp era obligatorio: no se puede volver si hay fichas con solo teléfono fijo.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "ProviderContacts" WHERE "PhoneNumber" IS NULL) THEN
                        RAISE EXCEPTION 'Hay fichas con solo teléfono fijo: no se puede volver al esquema anterior.';
                    END IF;
                END $$;
                """);

            migrationBuilder.AddColumn<string>(
                name: "InstagramUsername",
                table: "Providers",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Providers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "Providers" p
                SET "PhoneNumber" = c."PhoneNumber", "InstagramUsername" = c."InstagramUsername"
                FROM "ProviderContacts" c
                WHERE c."ProviderId" = p."Id";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Providers_InstagramUsername",
                table: "Providers",
                column: "InstagramUsername",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Providers_PhoneNumber",
                table: "Providers",
                column: "PhoneNumber");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Providers_InstagramUsername_Lower",
                table: "Providers",
                sql: "\"InstagramUsername\" IS NULL OR \"InstagramUsername\" = lower(\"InstagramUsername\")");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Providers_PhoneNumber",
                table: "Providers",
                sql: "\"PhoneNumber\" ~ '^\\+549\\d{10}$'");

            migrationBuilder.DropTable(name: "ProviderContacts");
        }
    }
}
