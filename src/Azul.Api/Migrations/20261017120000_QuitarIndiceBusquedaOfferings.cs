using Azul.Api.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Azul.Api.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20261017120000_QuitarIndiceBusquedaOfferings")]
    public partial class QuitarIndiceBusquedaOfferings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Con 109 filas Postgres igual hace Seq Scan, y la búsqueda de servicios ya no depende de este índice.
            // Se deja pg_trgm y f_unaccent: los usa f_matches y servirían para un índice en Providers
            // (ver docs/escalar-busqueda/LEEME.md).
            migrationBuilder.Sql("""DROP INDEX "IX_Offerings_Name_Search";""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 CREATE INDEX "IX_Offerings_Name_Search" ON "Offerings"
                                 USING gin (f_unaccent(lower("Name")) gin_trgm_ops);
                                 """);
        }
    }
}
