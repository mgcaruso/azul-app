using Microsoft.AspNetCore.Identity;

namespace Azul.Api.Common.Security;

public static class IdentityResultExtensions
{
    // Identity's managers return an IdentityResult instead of throwing: turn a failure into an exception.
    public static void EnsureSucceeded(this IdentityResult result)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Identity rejected the operation: {errors}");
        }
    }
}
