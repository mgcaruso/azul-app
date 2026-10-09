using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Azul.Api.Migrations
{
    public partial class IndiceUnicoNombreOfferingPorCategoria : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_Offerings_CategoryId_Name_Lower\" ON \"Offerings\" (\"CategoryId\", lower(\"Name\"));");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX \"IX_Offerings_CategoryId_Name_Lower\";");
        }
    }
}
