using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Azul.Api.Migrations
{
    /// <inheritdoc />
    public partial class LaunchCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Removes the test data: providers (with their contacts, consents and offerings) and the old catalog.
            // Users and roles stay. The new catalog is loaded on startup by CatalogSeeder, which runs on an
            // empty Categories table. No CASCADE on purpose: if another table references these, this fails
            // instead of silently emptying it.
            migrationBuilder.Sql("""
                                 TRUNCATE TABLE "ProviderOfferings", "ProviderContacts", "ProviderConsents",
                                                "Providers", "Offerings", "Categories"
                                 RESTART IDENTITY;
                                 """);

            migrationBuilder.AddColumn<List<string>>(
                name: "Keywords",
                table: "Offerings",
                type: "text[]",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The deleted test data is not restored.
            migrationBuilder.DropColumn(
                name: "Keywords",
                table: "Offerings");
        }
    }
}
