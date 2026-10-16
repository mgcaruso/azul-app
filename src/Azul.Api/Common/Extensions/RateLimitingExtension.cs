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

            options.AddPolicy("admin-login", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                        { PermitLimit = rateLimit.AdminLoginPerMinute, Window = TimeSpan.FromMinutes(1) }));

            options.AddPolicy("provider-signup", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                        { PermitLimit = rateLimit.SignupPerHour, Window = TimeSpan.FromHours(1) }));
        });
        return services;
    }

    // Misma forma que el resto de los errores de la API (ProblemDetails con `code`), para que el front la muestre.
    private static async ValueTask WriteRateLimitedResponse(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Detail = "Hiciste demasiados intentos. Esperá un rato y probá de nuevo."
        };
        problem.Extensions["code"] = "rate_limited";

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            problem, (JsonSerializerOptions?)null, "application/problem+json", cancellationToken);
    }
}
