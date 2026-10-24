using Azul.Api.Data;
using Azul.Api.Services.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Azul.Api.Tests.Auth;

// Levanta la API en memoria contra una base de tests propia (azul_tests), que se recrea en cada corrida.
public class AuthApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Por defecto, el Postgres de docker-compose. Se puede cambiar con la variable AZUL_TEST_DB.
    private readonly string connectionString = Environment.GetEnvironmentVariable("AZUL_TEST_DB")
        ?? "Host=localhost;Port=5433;Database=azul_tests;Username=azul;Password=azul_dev";

    public async Task InitializeAsync()
    {
        // Antes de levantar la API, porque al arrancar siembra el catálogo y necesita las tablas.
        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connectionString).Options;
        await using var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Default", connectionString);
        builder.UseSetting("Admin:ApiKey", "clave-de-tests");
        builder.UseSetting("RateLimit:GoogleSignInPerMinute", "1000");
        // En cada pedido, para que "cerrar sesión en todos lados" se note sin esperar 5 minutos.
        builder.UseSetting("Session:SecurityStampCheckMinutes", "0");

        builder.ConfigureTestServices(services =>
            services.AddSingleton<IGoogleTokenValidator, FakeGoogleTokenValidator>());
    }

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();
}
