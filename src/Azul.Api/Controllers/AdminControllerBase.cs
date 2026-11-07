using Azul.Api.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public abstract class AdminControllerBase : ControllerBase
{
    
}