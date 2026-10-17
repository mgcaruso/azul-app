using Azul.Api.Common.Security;
using Azul.Api.Data;
using Azul.Api.Services;
using Azul.Api.Services.Catalog;
using Azul.Api.Services.Images;
using Azul.Api.Services.Categories;
using Azul.Api.Services.Offerings;
using Azul.Api.Services.Providers;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Azul.Api.Common.Extensions;

public static class ServiceCollectionExtensions
{
    // Base de datos, servicios de la app y validadores.
    public static IServiceCollection AddAppServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var isDevelopment = configuration["ASPNETCORE_ENVIRONMENT"] == "Development";
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Default"));
            // Solo en desarrollo: muestra los valores de los parámetros del SQL en el log.
            if (isDevelopment)
            {
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IOfferingService, OfferingService>();
        services.AddScoped<IProviderReadService, ProviderReadService>();
        services.AddScoped<IProviderWriteService, ProviderWriteService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddSingleton<IPhoneHasher, PhoneHasher>();
        services.AddSingleton<ImageProcessor>();
        services.AddSingleton<IImageStorage, LocalImageStorage>();
        services.AddSingleton<AdminKeyFilter>();

        services.AddValidatorsFromAssemblyContaining<Program>();

        return services;
    }
}
