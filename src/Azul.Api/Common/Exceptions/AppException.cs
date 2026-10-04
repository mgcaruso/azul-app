namespace Azul.Api.Common.Exceptions;

// Base de todos los errores ESPERADOS (parte del negocio).
// No sabe nada de HTTP: el que decide el status es AppExceptionHandler.
// Code: estable, para que el front decida por el código y no por el texto.
// Message: llega tal cual al cliente, así que tiene que estar pensado para el usuario.
public abstract class AppException : Exception
{
    public string Code { get; }

    protected AppException(string code, string message) : base(message)
    {
        Code = code;
    }
}
