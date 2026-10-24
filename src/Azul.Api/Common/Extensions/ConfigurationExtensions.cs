namespace Azul.Api.Common.Extensions;

public static class ConfigurationExtensions
{
    // Falla al arrancar (no en el primer request) si falta una configuración imprescindible.
    // En producción van por variable de entorno: Photos__BaseUrl, Admin__ApiKey, Google__ClientId.
    public static void EnsureRequiredSettings(this IConfiguration configuration)
    {
        Require(configuration, "Photos:BaseUrl");
        Require(configuration, "Admin:ApiKey");
        Require(configuration, "Google:ClientId");
    }

    private static void Require(IConfiguration configuration, string key)
    {
        if (string.IsNullOrWhiteSpace(configuration[key]))
        {
            throw new InvalidOperationException($"Falta la configuración {key}.");
        }
    }
}
