using Azul.Api.Common.Exceptions;

namespace Azul.Api.Services.Errors;

// Catálogo de los errores de negocio que puede tirar Category.
// Cada método solo crea la excepción con su code y su mensaje; el service hace el throw.
public static class CategoryErrors
{
    public static NotFoundException NotFound(int id)
        => new("category.not_found", $"No existe la categoría {id}.");
}
