using Azul.Api.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddAppLogging();

builder.Configuration.EnsureRequiredSettings();

builder.Services
    .AddAppServices(builder.Configuration)
    .AddAppAuth(builder.Configuration)
    .AddAppControllers()
    .AddAppCors(builder.Configuration)
    .AddAppOpenApi()
    .AddAppRateLimiting(builder.Configuration);

var app = builder.Build();

await app.SeedCatalogAsync();

app.UseAppPipeline();
app.MapControllers();

app.Run();
