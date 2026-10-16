using Azul.Api.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddAppLogging();

builder.Configuration.EnsureRequiredSettings();

builder.Services
    .AddAppServices(builder.Configuration)
    .AddAppControllers()
    .AddAppCors()
    .AddAppOpenApi()
    .AddAppRateLimiting(builder.Configuration);

var app = builder.Build();

await app.SeedDevelopmentDataAsync();

app.UseAppPipeline();
app.MapControllers();

app.Run();
