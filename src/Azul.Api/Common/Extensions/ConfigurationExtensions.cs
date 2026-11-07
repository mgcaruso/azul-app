namespace Azul.Api.Common.Extensions;

public static class ConfigurationExtensions
{
    
    // Falla al arrancar (no en el primer request) si falta una configuración imprescindible.
    // En producción van por variable de entorno: Photos__BaseUrl, Google__ClientId, Admin__BootstrapEmails__0.
    public static void EnsureRequiredSettings(this IConfiguration configuration)
    {
        Require(configuration, "Photos:BaseUrl");
        Require(configuration, "Google:ClientId");
        RequireNonEmptyList(configuration.GetSection("Admin:BootstrapEmails"));
    }

    // A list setting (Admin__BootstrapEmails__0, __1...) must have at least one non-blank entry.
    private static void RequireNonEmptyList(IConfigurationSection section)
    {
        var values = section.Get<string[]>();
        if (values is null || values.Length == 0 || values.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException($"Missing configuration {section.Path}.");
        }
    }

    private static void Require(IConfiguration configuration, string key)
    {
        if (string.IsNullOrWhiteSpace(configuration[key]))
        {
            throw new InvalidOperationException($"Falta la configuración {key}.");
        }
    }
}
