using System.Security.Claims;
using Azul.Api.Common.Exceptions;
using Azul.Api.Common.Security;
using Azul.Api.Data;
using Azul.Api.DTOs.Auth;
using Azul.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Azul.Api.Services.Auth;

public class AuthService(
    IGoogleTokenValidator googleTokenValidator,
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    IConfiguration configuration,
    AppDbContext dbContext) : IAuthService
{
    public async Task<MeDto> SignInWithGoogleAsync(string idToken)
    {
        var googleUser = await googleTokenValidator.ValidateAsync(idToken);
        if (googleUser is null)
        {
            throw UnauthorizedException.InvalidGoogleToken();
        }

        var appUser = await userManager.FindByLoginAsync("Google", googleUser.Subject);
        if (appUser is null)
        {
            appUser = new AppUser
            {
                DisplayName = googleUser.Name,
                UserName = googleUser.Email,
                PictureUrl = googleUser.PictureUrl,
                Email = googleUser.Email,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };
            (await userManager.CreateAsync(appUser)).EnsureSucceeded();
            (await userManager.AddLoginAsync(
                appUser, new UserLoginInfo("Google", googleUser.Subject, "Google"))).EnsureSucceeded();
        }
        else
        {
            var hasInfoChanged = (appUser.Email, appUser.DisplayName, appUser.PictureUrl)
                                 != (googleUser.Email, googleUser.Name, googleUser.PictureUrl);
            if (hasInfoChanged)
            {
                appUser.Email = googleUser.Email;
                appUser.UserName = googleUser.Email;
                appUser.DisplayName = googleUser.Name;
                appUser.PictureUrl = googleUser.PictureUrl;
                (await userManager.UpdateAsync(appUser)).EnsureSucceeded();
            }
        }

        // Before SignInAsync: the session cookie is built with the roles the user has at this moment.
        await EnsureBootstrapAdminAsync(appUser);
        await signInManager.SignInAsync(appUser, isPersistent: true);
        return await ToMeDtoAsync(appUser);
    }

    // The first admin comes from configuration (Admin:BootstrapEmails): if the user who just signed in
    // is listed and does not have the Admin role yet, it is created (once) and assigned.
    // Every other admin is managed from the admin panel.
    private async Task EnsureBootstrapAdminAsync(AppUser appUser)
    {
        var bootstrapEmails = configuration.GetSection("Admin:BootstrapEmails").Get<string[]>() ?? [];
        var isBootstrapAdmin = bootstrapEmails.Contains(appUser.Email, StringComparer.OrdinalIgnoreCase);
        if (!isBootstrapAdmin || await userManager.IsInRoleAsync(appUser, AppRoles.Admin))
        {
            return;
        }

        if (!await roleManager.RoleExistsAsync(AppRoles.Admin))
        {
            (await roleManager.CreateAsync(new IdentityRole<Guid>(AppRoles.Admin))).EnsureSucceeded();
        }

        (await userManager.AddToRoleAsync(appUser, AppRoles.Admin)).EnsureSucceeded();
    }

    // Un solo lugar donde se arma lo que el front sabe de la sesión.
    private async Task<MeDto> ToMeDtoAsync(AppUser appUser)
    {
        
        var isProvider = await dbContext.Providers.AnyAsync(p => p.UserId == appUser.Id);
        var isAdmin = await userManager.IsInRoleAsync(appUser, AppRoles.Admin);
        return new MeDto(appUser.Id, appUser.Email!, appUser.DisplayName, appUser.PictureUrl, isProvider, isAdmin);
    }

    public async Task<MeDto> GetMeAsync(ClaimsPrincipal principal)
    {
        // GetUserAsync saca el id que viene dentro de la cookie y busca el user en la base.
        var appUser = await userManager.GetUserAsync(principal);
        if (appUser is null)
        {
            throw UnauthorizedException.NotSignedIn();
        }

        return await ToMeDtoAsync(appUser);
    }

    public Task SignOutAsync() => signInManager.SignOutAsync();

    public async Task SignOutEverywhereAsync(ClaimsPrincipal principal)
    {
        var appUser = await userManager.GetUserAsync(principal);
        if (appUser is null)
        {
            throw UnauthorizedException.NotSignedIn();
        }

        (await userManager.UpdateSecurityStampAsync(appUser)).EnsureSucceeded();
        await signInManager.SignOutAsync();
    }

}