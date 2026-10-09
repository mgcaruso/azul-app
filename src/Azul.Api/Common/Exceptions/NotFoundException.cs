namespace Azul.Api.Common.Exceptions;

public class NotFoundException : AppException
{
    public string Resource { get; }
    public object Id { get; }

    public NotFoundException(string resource, object id)
        : base("not_found", $"No existe {resource} con id {id}.")
    {
        Resource = resource;
        Id = id;
    }
}
