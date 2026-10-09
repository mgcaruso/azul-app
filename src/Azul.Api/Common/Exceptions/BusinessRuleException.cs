namespace Azul.Api.Common.Exceptions;

public class BusinessRuleException : AppException
{
    public BusinessRuleException(string code, string message) : base(code, message)
    {
    }
}
