using System.Text.Json.Serialization;
using Azul.Api.Common.Binding;
using Azul.Api.Common.ErrorHandling;
using Azul.Api.Common.Validation;

namespace Azul.Api.Common.Extensions;

public static class ControllersExtensions
{
    // Controllers, JSON, validación y manejo de errores (ProblemDetails).
    public static IServiceCollection AddAppControllers(this IServiceCollection services)
    {
        services.AddControllers(options =>
            {
                options.Filters.Add<ValidationFilter>();
                options.ModelMetadataDetailsProviders.Add(new CamelCaseQueryNamesProvider());
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                ModelStateValidation.UseSpanishMessages(options.ModelBindingMessageProvider);
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.AllowInputFormatterExceptionMessages = false;
            })
            .ConfigureApiBehaviorOptions(options =>
                options.InvalidModelStateResponseFactory = context =>
                    throw ModelStateValidation.ToException(context.ModelState));

        services.AddExceptionHandler<AppExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
