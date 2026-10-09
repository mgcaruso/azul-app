using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Azul.Api.Migrations
{
    public partial class QuitarIndiceUnicoCategorias : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Categories_Name_Lower";""");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""CREATE UNIQUE INDEX IF NOT EXISTS "IX_Categories_Name_Lower" ON "Categories" (lower("Name"));""");
        }
    }
}
