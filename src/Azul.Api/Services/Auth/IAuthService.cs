using System.Security.Claims;
using Azul.Api.DTOs.Auth;

namespace Azul.Api.Services.Auth;

public interface IAuthService
{
    Task<MeDto> SignInWithGoogleAsync(string idToken);
    Task<MeDto> GetMeAsync(ClaimsPrincipal principal);
    Task SignOutAsync();
    Task SignOutEverywhereAsync(ClaimsPrincipal principal);
}