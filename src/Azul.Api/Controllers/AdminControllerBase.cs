using Azul.Api.Common.Security;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.Controllers;

[ServiceFilter(typeof(AdminKeyFilter))]
public abstract class AdminControllerBase : ControllerBase
{
    
}