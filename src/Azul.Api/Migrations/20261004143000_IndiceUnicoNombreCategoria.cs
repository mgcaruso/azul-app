using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Azul.Api.Migrations
{
    public partial class IndiceUnicoNombreCategoria : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_Categories_Name_Lower\" ON \"Categories\" (lower(\"Name\"));");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX \"IX_Categories_Name_Lower\";");
        }
    }
}
