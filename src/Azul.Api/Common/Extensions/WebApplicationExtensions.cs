using Azul.Api.Common.Logging;
using Azul.Api.Data;
using Azul.Api.Data.Seed;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Events;

namespace Azul.Api.Common.Extensions;

public static class WebApplicationExtensions
{
    // Middleware en el orden en que se ejecutan; los controllers se mapean aparte.
    public static WebApplication UseAppPipeline(this WebApplication app)
    {
        var forwardedHeaders = new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedProto };
        forwardedHeaders.KnownIPNetworks.Clear();
        forwardedHeaders.KnownProxies.Clear();
        app.UseForwardedHeaders(forwardedHeaders);

        // Primero, para registrar cada pedido (método, ruta, código y duración); la query string va como propiedad aparte.
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (httpContext, _, exception) => exception is not null || httpContext.Response.StatusCode >= 500
                ? LogEventLevel.Error
                : httpContext.Response.StatusCode >= 400
                    ? LogEventLevel.Warning
                    : LogEventLevel.Information;

            // Datos extra de cada pedido, para filtrar y armar columnas en Seq.
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("QueryString", httpContext.Request.QueryString.Value);
                diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString());
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                diagnosticContext.Set("IsAdmin", httpContext.Request.Path.StartsWithSegments("/api/admin"));
            };
        });

        // Solo en desarrollo: el cuerpo de cada pedido en el log (trae datos personales, no va a producción).
        if (app.Environment.IsDevelopment())
        {
            app.UseMiddleware<RequestBodyLoggingMiddleware>();
        }

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

        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    // Carga las categorías y servicios si la base está vacía: siempre en desarrollo y, en otros entornos, con Seed:Catalog=true.
    public static async Task SeedCatalogAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment() && !app.Configuration.GetValue<bool>("Seed:Catalog"))
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await DevSeeder.SeedAsync(dbContext);
    }
}
