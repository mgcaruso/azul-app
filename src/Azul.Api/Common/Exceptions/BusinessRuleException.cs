namespace Azul.Api.Common.Exceptions;

// 422: el pedido está bien formado, pero una regla del negocio no lo permite
// (ej. publicar un proveedor suspendido).
public class BusinessRuleException : AppException
{
    public BusinessRuleException(string code, string message) : base(code, message)
    {
    }
}
