namespace Azul.Api.Common.Exceptions;

// 403: el usuario no tiene permiso (ej. editar el perfil de otro proveedor, cuando llegue el login).
public class ForbiddenException : AppException
{
    public ForbiddenException(string code, string message) : base(code, message)
    {
    }
}
