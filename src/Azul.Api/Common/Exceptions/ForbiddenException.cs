namespace Azul.Api.Common.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(string code, string message) : base(code, message)
    {
    }
}
