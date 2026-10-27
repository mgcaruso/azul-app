using System.Security.Claims;
using Azul.Api.Common.Exceptions;

namespace Azul.Api.Common.Security;

public static class ClaimsPrincipalExtensions
{
   
    
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            throw UnauthorizedException.NotSignedIn();
        }
        return userId;
    }
}