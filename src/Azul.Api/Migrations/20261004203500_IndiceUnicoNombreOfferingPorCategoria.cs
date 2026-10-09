using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Azul.Api.Migrations
{
    // Escrita a mano: EF no sabe modelar un índice sobre lower("Name").
    // Un nombre de servicio es único dentro de su categoría, sin distinguir mayúsculas.
    // El modelo de EF no cambia, por eso el Designer es copia del de la migración anterior.
    /// <inheritdoc />
    public partial class IndiceUnicoNombreOfferingPorCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_Offerings_CategoryId_Name_Lower\" ON \"Offerings\" (\"CategoryId\", lower(\"Name\"));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX \"IX_Offerings_CategoryId_Name_Lower\";");
        }
    }
}
