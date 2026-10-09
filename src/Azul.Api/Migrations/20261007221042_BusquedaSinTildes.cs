using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Azul.Api.Migrations
{
    /// <inheritdoc />
    public partial class BusquedaSinTildes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");
            migrationBuilder.Sql("""
                                 CREATE OR REPLACE FUNCTION f_unaccent(text) RETURNS text
                                 LANGUAGE sql IMMUTABLE PARALLEL SAFE STRICT
                                 AS $$ SELECT public.unaccent('public.unaccent', $1) $$;
                                 """);

            migrationBuilder.Sql("""
                                 CREATE INDEX "IX_Offerings_Name_Search" ON "Offerings"
                                 USING gin (f_unaccent(lower("Name")) gin_trgm_ops);
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Primero lo que depende de las extensiones (índice y función), después las extensiones.
            migrationBuilder.Sql("""DROP INDEX "IX_Offerings_Name_Search";""");
            migrationBuilder.Sql("DROP FUNCTION f_unaccent(text);");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:unaccent", ",,");
        }
    }
}
