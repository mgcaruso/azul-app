using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Azul.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Providers_Status",
                table: "Providers");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Providers_Status",
                table: "Providers",
                sql: "\"Status\" IN ('Pending', 'Published', 'Suspended', 'Rejected')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Providers_Status",
                table: "Providers");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Providers_Status",
                table: "Providers",
                sql: "\"Status\" IN ('Pending', 'Published', 'Suspended')");
        }
    }
}
