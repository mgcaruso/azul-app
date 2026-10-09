namespace Azul.Api.Common.Exceptions;

public class ConflictException : AppException
{
    public string? Field { get; }

    public ConflictException(string code, string message, string? field = null) : base(code, message)
    {
        Field = field;
    }

    public static ConflictException Duplicate(string field)
        => new("duplicate_value", "Ya existe un registro con ese valor.", field);
}
