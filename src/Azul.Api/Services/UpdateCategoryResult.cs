namespace Azul.Api.Services;

// Qué pasó al editar una categoría, en idioma de negocio (no HTTP).
// El controller lo traduce a 204, 404 o 409.
public enum UpdateCategoryResult
{
    Updated,
    NotFound,
    DuplicateName
}
