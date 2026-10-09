using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Azul.Api.Migrations
{
    public partial class QuitarImageUrlIndiceNombre : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Categories");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Categories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
