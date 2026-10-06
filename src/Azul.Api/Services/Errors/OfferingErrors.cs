using Azul.Api.Common.Exceptions;

namespace Azul.Api.Services.Errors;

// Catálogo de los errores de negocio que puede tirar Offering.
// Cada método solo crea la excepción con su code y su mensaje; el service hace el throw.
public static class OfferingErrors
{
    // 404: GetById, Update y Delete con un id que no existe.
    public static NotFoundException NotFound(int id)
        => new("offering.not_found", $"No existe el servicio {id}.");

    // 400: el CategoryId del body no existe. Es un dato inválido del pedido (no un 404 del recurso),
    // por eso va como error de validación sobre el campo categoryId.
    public static ValidationFailedException CategoryNotFound(int categoryId)
        => new(new Dictionary<string, string[]>
        {
            ["categoryId"] = [$"No existe la categoría {categoryId}."]
        });
}
