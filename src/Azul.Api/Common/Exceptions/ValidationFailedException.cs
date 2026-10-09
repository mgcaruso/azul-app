namespace Azul.Api.Common.Exceptions;

public class ValidationFailedException : AppException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationFailedException(Dictionary<string, string[]> errors)
        : base("validation.failed", "Hay datos inválidos.")
    {
        Errors = errors;
    }

    public static ValidationFailedException ForField(string field, string message)
        => new(new Dictionary<string, string[]> { [field] = [message] });
}
