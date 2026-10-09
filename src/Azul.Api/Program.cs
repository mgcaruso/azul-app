using Azul.Api.Common.ErrorHandling;
using Azul.Api.Data;
using Azul.Api.Data.Seed;
using System.Text.Json.Serialization;
using Azul.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOfferingService, OfferingService>();
builder.Services.AddScoped<IProviderService, ProviderService>();

// Los enums viajan en el JSON como texto ("Business") y no como número.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Manejo global de errores: AppExceptionHandler traduce las excepciones a ProblemDetails (RFC 9457).
builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    // Los 400 de validación automáticos ([Required], [MaxLength]) no traen code:
    // se lo agregamos para que el front reciba siempre la misma forma.
    options.CustomizeProblemDetails = context =>
    {
        if (context.ProblemDetails is ValidationProblemDetails)
        {
            context.ProblemDetails.Extensions["code"] = "validation.failed";
        }
    };
});

builder.Services.AddOpenApi();

var app = builder.Build();

// Bien arriba, para que atrape las excepciones de todo lo que viene después.
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Datos de prueba: solo en desarrollo y solo si la base está vacía.
    // TODO (Guada): descomentar cuando DevSeeder esté listo (hoy tira NotImplementedException).
    // using (var scope = app.Services.CreateScope())
    // {
    //     var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    //     await DevSeeder.SeedAsync(dbContext);
    // }

    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Azul API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
