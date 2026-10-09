using Azul.Api.Common.Binding;
using Azul.Api.Common.ErrorHandling;
using Azul.Api.Data;
using Azul.Api.Data.Seed;
using System.Text.Json.Serialization;
using Azul.Api.Services;
using Azul.Api.Common.Validation;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOfferingService, OfferingService>();
builder.Services.AddScoped<IProviderService, ProviderService>();

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

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
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
