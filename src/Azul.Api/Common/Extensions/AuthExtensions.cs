using Azul.Api.Common.Security;
using Azul.Api.Data;
using Azul.Api.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

namespace Azul.Api.Common.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddAppAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var session = configuration.GetSection("Session").Get<AuthSessionOptions>() ?? new AuthSessionOptions();

        services.AddIdentityCore<AppUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager();

        services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddIdentityCookies();

        services.ConfigureApplicationCookie(o =>
        {
            o.Cookie.Name = "azul.session";
            o.Cookie.HttpOnly = true;
            o.Cookie.SameSite = SameSiteMode.Lax;
            o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            o.ExpireTimeSpan = TimeSpan.FromDays(session.ExpireDays);
            o.SlidingExpiration = true;
            // Es una API: en vez de redirigir a una página de login, responde el código.
            o.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
            o.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
        });

        // Si el SecurityStamp de la base cambió, la cookie deja de valer.
        services.Configure<SecurityStampValidatorOptions>(o =>
            o.ValidationInterval = TimeSpan.FromMinutes(session.SecurityStampCheckMinutes));

        // Las claves que cifran la cookie van a la base, para que sobrevivan a los reinicios de Cloud Run.
        services.AddDataProtection()
            .PersistKeysToDbContext<AppDbContext>()
            .SetApplicationName("azul-api");

        return services;
    }
}