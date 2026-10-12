using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Azul.Api.Migrations
{
    /// <inheritdoc />
    public partial class FuncionBusquedaTexto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Lo que usa SearchText.Matches: minúsculas, sin tildes y LIKE en una sola función.
            // Es LANGUAGE sql e IMMUTABLE para que Postgres la reemplace por la expresión de adentro
            // y siga usando los índices sobre f_unaccent(lower(columna)), como IX_Offerings_Name_Search.
            migrationBuilder.Sql("""
                                 CREATE OR REPLACE FUNCTION f_matches(value text, pattern text) RETURNS boolean
                                 LANGUAGE sql IMMUTABLE PARALLEL SAFE
                                 AS $$ SELECT f_unaccent(lower(value)) LIKE f_unaccent(pattern) $$;
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION f_matches(text, text);");
        }
    }
}
