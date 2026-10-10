using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Azul.Api.Migrations
{
    /// <inheritdoc />
    public partial class ModeloProveedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Providers_Type",
                table: "Providers");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Providers",
                newName: "DisplayName");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Providers",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Providers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Providers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Providers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstPublishedAt",
                table: "Providers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hours",
                table: "Providers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstagramUsername",
                table: "Providers",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Providers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Providers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhotoKey",
                table: "Providers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Providers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Providers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "ProviderConsents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProviderId = table.Column<int>(type: "integer", nullable: true),
                    PhoneHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderConsents", x => x.Id);
                    table.CheckConstraint("CK_ProviderConsents_RevokedAt", "\"RevokedAt\" IS NULL OR \"RevokedAt\" >= \"AcceptedAt\"");
                    table.ForeignKey(
                        name: "FK_ProviderConsents_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Providers_FirstPublishedAt",
                table: "Providers",
                column: "FirstPublishedAt",
                descending: new bool[0],
                filter: "\"Status\" = 'Published'");

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
                name: "CK_Providers_Address_RequiredBusiness",
                table: "Providers",
                sql: "\"Type\" <> 'Business' OR \"Address\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Providers_DisplayName_NotBlank",
                table: "Providers",
                sql: "btrim(\"DisplayName\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Providers_FirstPublishedAt",
                table: "Providers",
                sql: "\"Status\" <> 'Published' OR \"FirstPublishedAt\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Providers_InstagramUsername_Lower",
                table: "Providers",
                sql: "\"InstagramUsername\" IS NULL OR \"InstagramUsername\" = lower(\"InstagramUsername\")");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Providers_Name_OnlyIndividual",
                table: "Providers",
                sql: "\"Type\" = 'Individual' OR (\"FirstName\" IS NULL AND \"LastName\" IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Providers_Name_RequiredIndividual",
                table: "Providers",
                sql: "\"Type\" <> 'Individual' OR (\"FirstName\" IS NOT NULL AND \"LastName\" IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Providers_PhoneNumber",
                table: "Providers",
                sql: "\"PhoneNumber\" ~ '^\\+549\\d{10}$'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Providers_Status",
                table: "Providers",
                sql: "\"Status\" IN ('Pending', 'Published', 'Suspended')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Providers_Type",
                table: "Providers",
                sql: "\"Type\" IN ('Individual', 'Venture', 'Business')");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderConsents_ProviderId",
                table: "ProviderConsents",
                column: "ProviderId",
                unique: true,
                filter: "\"RevokedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderConsents");

            migrationBuilder.DropIndex(
                name: "IX_Providers_FirstPublishedAt",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_Providers_InstagramUsername",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_Providers_PhoneNumber",
                table: "Providers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Providers_Address_RequiredBusiness",
                table: "Providers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Providers_DisplayName_NotBlank",
                table: "Providers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Providers_FirstPublishedAt",
                table: "Providers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Providers_InstagramUsername_Lower",
                table: "Providers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Providers_Name_OnlyIndividual",
                table: "Providers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Providers_Name_RequiredIndividual",
                table: "Providers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Providers_PhoneNumber",
                table: "Providers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Providers_Status",
                table: "Providers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Providers_Type",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "FirstPublishedAt",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "Hours",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "InstagramUsername",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "PhotoKey",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Providers");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "Providers",
                newName: "Name");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Providers_Type",
                table: "Providers",
                sql: "\"Type\" IN ('Individual', 'Business')");
        }
    }
}
