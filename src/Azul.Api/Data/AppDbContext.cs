using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Azul.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Offering> Offerings => Set<Offering>();
}