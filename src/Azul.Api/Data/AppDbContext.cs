using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Azul.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Offering> Offerings => Set<Offering>();
    
    // Para usarla en LINQ. Nunca se ejecuta en C#: EF la traduce a f_unaccent(...) en SQL.
    public static string FUnaccent(string text) => throw new NotSupportedException();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("unaccent");
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.HasDbFunction(() => FUnaccent(default!)).HasName("f_unaccent");
    }
}