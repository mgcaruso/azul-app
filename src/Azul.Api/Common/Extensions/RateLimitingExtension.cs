using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;
using Azul.Api.Common.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Azul.Api.Common.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddAppRateLimiting(
        this IServiceCollection services, IConfiguration configuration)
    {
        var rateLimit = configuration.GetSection("RateLimit").Get<RateLimitOptions>() ?? new RateLimitOptions();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = WriteRateLimitedResponse;

            // Freno general por IP para todo /api: alguien que hace cientos de pedidos por minuto no es una persona.
            // Corre en todos los pedidos, además de las políticas de cada endpoint. Solo /api: el front y las fotos no pasan por acá.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                context.Request.Path.StartsWithSegments("/api")
                    ? RateLimitPartition.GetFixedWindowLimiter(
                        $"public-ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}",
                        _ => new FixedWindowRateLimiterOptions
                            { PermitLimit = rateLimit.PublicPerMinute, Window = TimeSpan.FromMinutes(1) })
                    : RateLimitPartition.GetNoLimiter("sin-limite"));

            options.AddPolicy("provider-signup", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.User.Identity?.IsAuthenticated == true
                        ? $"user:{context.User.FindFirstValue(ClaimTypes.NameIdentifier)}"
                        : $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}",
                    _ => new FixedWindowRateLimiterOptions
                        { PermitLimit = rateLimit.SignupPerHour, Window = TimeSpan.FromHours(1) }));

            options.AddPolicy("photo-upload", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                        { PermitLimit = rateLimit.PhotoUploadPerHour, Window = TimeSpan.FromHours(1) }));

            options.AddPolicy("search", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                        { PermitLimit = rateLimit.SearchPerMinute, Window = TimeSpan.FromMinutes(1) }));

            options.AddPolicy("google-sign-in", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                        { PermitLimit = rateLimit.GoogleSignInPerMinute, Window = TimeSpan.FromMinutes(1) }));
        });
        return services;
    }

    // Misma forma que el resto de los errores de la API (ProblemDetails con `code`). El texto que ve la persona
    // lo arma el front a partir del `code` y de `retryAfterSeconds`; `detail` es solo para quien lee la respuesta.
    private static async ValueTask WriteRateLimitedResponse(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Detail = "Too many requests."
        };
        problem.Extensions["code"] = "rate_limited";

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            var seconds = (int)Math.Ceiling(retryAfter.TotalSeconds);
            context.HttpContext.Response.Headers.RetryAfter = seconds.ToString();
            problem.Extensions["retryAfterSeconds"] = seconds;
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            problem, (JsonSerializerOptions?)null, "application/problem+json", cancellationToken);
    }
}
