using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Azul.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarProveedores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Providers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers", x => x.Id);
                    table.CheckConstraint("CK_Providers_Type", "\"Type\" IN ('Individual', 'Business')");
                });

            migrationBuilder.CreateTable(
                name: "ProviderOfferings",
                columns: table => new
                {
                    ProviderId = table.Column<int>(type: "integer", nullable: false),
                    OfferingId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderOfferings", x => new { x.ProviderId, x.OfferingId });
                    table.ForeignKey(
                        name: "FK_ProviderOfferings_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderOfferings_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderOfferings_OfferingId",
                table: "ProviderOfferings",
                column: "OfferingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderOfferings");

            migrationBuilder.DropTable(
                name: "Providers");
        }
    }
}
