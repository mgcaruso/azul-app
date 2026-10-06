namespace Azul.Api.Common.Exceptions;

// 400: datos inválidos que se chequean en el service (los de [Required]/[MaxLength] los responde ASP.NET solo).
// Se llama ValidationFailedException para no chocar con System.ComponentModel.DataAnnotations.ValidationException.
public class ValidationFailedException : AppException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationFailedException(Dictionary<string, string[]> errors)
        : base("validation.failed", "Hay datos inválidos.")
    {
        Errors = errors;
    }

    // Atajo para el caso más común: un solo campo inválido (ej. un categoryId que no existe).
    public static ValidationFailedException ForField(string field, string message)
        => new(new Dictionary<string, string[]> { [field] = [message] });
}
