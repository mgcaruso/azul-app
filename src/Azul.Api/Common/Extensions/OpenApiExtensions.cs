using System.Text.Json.Serialization;
using Microsoft.OpenApi;

namespace Azul.Api.Common.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddAppOpenApi(this IServiceCollection services)
    {
        // El documento OpenAPI toma las opciones de JSON de acá, no de AddJsonOptions de MVC.
        // Sin esto los enums salen como número y los int como "number | string" en los tipos del front.
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
        });

        services.AddOpenApi(options =>
        {
            options.AddSchemaTransformer((schema, context, _) =>
            {
                if (schema.Properties is null)
                {
                    return Task.CompletedTask;
                }

                foreach (var jsonProperty in context.JsonTypeInfo.Properties)
                {
                    if (!schema.Properties.TryGetValue(jsonProperty.Name, out var property))
                    {
                        continue;
                    }

                    var clrType = jsonProperty.PropertyType;
                    var isNonNullableValueType = clrType.IsValueType && Nullable.GetUnderlyingType(clrType) is null;
                    if (isNonNullableValueType || property.Type is { } type && !type.HasFlag(JsonSchemaType.Null))
                    {
                        schema.Required ??= new HashSet<string>();
                        schema.Required.Add(jsonProperty.Name);
                    }
                }
                return Task.CompletedTask;
            });
        });

        return services;
    }
}
