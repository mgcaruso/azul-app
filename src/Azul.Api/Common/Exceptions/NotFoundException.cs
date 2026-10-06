namespace Azul.Api.Common.Exceptions;

// 404: el recurso no existe. Genérica: se le pasa qué recurso y qué id, no un texto.
// El front decide qué mostrar con code + resource; detail es solo para quien lee la respuesta.
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
