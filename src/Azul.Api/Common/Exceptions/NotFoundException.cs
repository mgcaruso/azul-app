namespace Azul.Api.Common.Exceptions;

// 404: el recurso no existe.
public class NotFoundException : AppException
{
    public NotFoundException(string code, string message) : base(code, message)
    {
    }
}
