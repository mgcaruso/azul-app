using System.Security.Claims;
using Azul.Api.Common.Exceptions;
using Azul.Api.DTOs.Auth;
using Azul.Api.Entities;
using Microsoft.AspNetCore.Identity;

namespace Azul.Api.Services.Auth;

public class AuthService(
    IGoogleTokenValidator googleTokenValidator,
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager) : IAuthService
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
            EnsureSucceeded(await userManager.CreateAsync(appUser));
            EnsureSucceeded(await userManager.AddLoginAsync(
                appUser, new UserLoginInfo("Google", googleUser.Subject, "Google")));
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
                EnsureSucceeded(await userManager.UpdateAsync(appUser));
            }
        }

        await signInManager.SignInAsync(appUser, isPersistent: true);
        return new MeDto(appUser.Id, googleUser.Email, appUser.DisplayName, appUser.PictureUrl);
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Identity rechazó la operación: {errors}");
        }
    }


    public async Task<MeDto> GetMeAsync(ClaimsPrincipal principal)
    {
        // GetUserAsync saca el id que viene dentro de la cookie y busca el user en la base.
        var appUser = await userManager.GetUserAsync(principal);
        if (appUser is null)
        {
            throw UnauthorizedException.NotSignedIn();
        }

        return new MeDto(appUser.Id, appUser.Email!, appUser.DisplayName, appUser.PictureUrl);
    }

    public Task SignOutAsync() => signInManager.SignOutAsync();

    public async Task SignOutEverywhereAsync(ClaimsPrincipal principal)
    {
        var appUser = await userManager.GetUserAsync(principal);
        if (appUser is null)
        {
            throw UnauthorizedException.NotSignedIn();
        }

        EnsureSucceeded(await userManager.UpdateSecurityStampAsync(appUser));
        await signInManager.SignOutAsync();
    }
}
