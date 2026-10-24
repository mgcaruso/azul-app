namespace Azul.Api.Common.Extensions;

public static class CorsExtensions
{
    public const string FrontendPolicy = "Frontend";

    public static IServiceCollection AddAppCors(
        this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:5173"];

        services.AddCors(options =>
            options.AddPolicy(FrontendPolicy, policy => policy
                .WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));

        return services;
    }
}
