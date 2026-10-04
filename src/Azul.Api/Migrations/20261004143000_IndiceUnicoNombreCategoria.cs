using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Azul.Api.Migrations
{
    /// <inheritdoc />
    public partial class IndiceUnicoNombreCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Índice único sin distinguir mayúsculas: "Plomería" y "plomería" chocan.
            // EF no genera índices sobre expresiones como lower(...), por eso va en SQL.
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_Categories_Name_Lower\" ON \"Categories\" (lower(\"Name\"));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX \"IX_Categories_Name_Lower\";");
        }
    }
}
