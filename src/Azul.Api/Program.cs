using Azul.Api.Common.Binding;
using Azul.Api.Common.ErrorHandling;
using Azul.Api.Data;
using Azul.Api.Data.Seed;
using System.Text.Json.Serialization;using Azul.Api.Services;
using Azul.Api.Common.Validation;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOfferingService, OfferingService>();
builder.Services.AddScoped<IProviderService, ProviderService>();
builder.Services.AddSingleton<IPhoneHasher, PhoneHasher>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddControllers(options =>
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

builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails();

const string FrontendCorsPolicy = "Frontend";
builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()));

// El documento OpenAPI toma las opciones de JSON de acá, no de AddJsonOptions de MVC.
// Sin esto los enums salen como número y los int como "number | string" en los tipos del front.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

builder.Services.AddOpenApi(options =>
    options.AddSchemaTransformer((schema, context, _) =>
    {
        // Marca como obligatorias las propiedades que no aceptan null (id, type, offerings...),
        // para que en el front no lleguen todas como opcionales.
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
    }));

var app = builder.Build();

app.UseCors(FrontendCorsPolicy);

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await DevSeeder.SeedAsync(dbContext);
    }

    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Azul API v1");
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Mientras no haya CDN, las fotos se sirven desde wwwroot/photos.
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
