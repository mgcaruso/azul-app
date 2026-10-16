using Azul.Api.Data;
using Azul.Api.Data.Seed;

namespace Azul.Api.Common.Extensions;

public static class WebApplicationExtensions
{
    // Middleware en el orden en que se ejecutan; los controllers se mapean aparte.
    public static WebApplication UseAppPipeline(this WebApplication app)
    {
        app.UseCors(CorsExtensions.FrontendPolicy);
        app.UseRateLimiter();

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "Azul API v1");
                // Recuerda la clave en el navegador para no pegarla en cada recarga.
                options.EnablePersistAuthorization();
            });
        }
        else
        {
            app.UseHttpsRedirection();
        }

        // Mientras no haya CDN, las fotos se sirven desde wwwroot/photos.
        app.UseStaticFiles();

        app.UseAuthorization();

        return app;
    }

    // Solo en desarrollo: carga el catálogo y proveedores de prueba.
    public static async Task SeedDevelopmentDataAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await DevSeeder.SeedAsync(dbContext);
    }
}
