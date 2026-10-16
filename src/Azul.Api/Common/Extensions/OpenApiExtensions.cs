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
            // Declara la clave de admin (header X-Admin-Key) para que Swagger muestre el botón Authorize.
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["AdminKey"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    Name = "X-Admin-Key",
                    In = ParameterLocation.Header,
                    Description = "Clave de admin (la misma de user-secrets: Admin:ApiKey).",
                };
                return Task.CompletedTask;
            });

            // Solo las rutas /api/admin piden la clave.
            options.AddOperationTransformer((operation, context, _) =>
            {
                if (context.Description.RelativePath?.StartsWith("api/admin") == true)
                {
                    operation.Security ??= [];
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("AdminKey", context.Document)] = []
                    });
                }
                return Task.CompletedTask;
            });

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
