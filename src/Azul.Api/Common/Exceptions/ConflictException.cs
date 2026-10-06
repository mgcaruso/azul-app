namespace Azul.Api.Common.Exceptions;

// 409: choca con algo que ya existe (duplicados, dependencias).
public class ConflictException : AppException
{
    // Campo técnico afectado ("name"), para que el front marque el input. Opcional.
    public string? Field { get; }

    public ConflictException(string code, string message, string? field = null) : base(code, message)
    {
        Field = field;
    }

    // Duplicado genérico: el front mapea por code + field, no muestra el texto del backend.
    public static ConflictException Duplicate(string field)
        => new("duplicate_value", "Ya existe un registro con ese valor.", field);
}
