using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Azul.Api.Common.Security;

public class AdminKeyFilter(IConfiguration configuration) : IAuthorizationFilter 
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var receivedKey = context.HttpContext.Request.Headers["X-Admin-Key"].ToString();
        var expectedKey = configuration["Admin:ApiKey"]!;
        if (string.IsNullOrEmpty(receivedKey))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var receivedBytes = Encoding.UTF8.GetBytes(receivedKey);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedKey);
        var isValid = CryptographicOperations.FixedTimeEquals(receivedBytes, expectedBytes);
        if (!isValid)
        {
           context.Result = new UnauthorizedResult();
        }
    }
}