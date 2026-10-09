using Azul.Api.Common.Pagination;
using Azul.Api.Entities;

namespace Azul.Api.DTOs;

// Parámetros de GET /api/providers?type=&page=&pageSize=
// Todos opcionales: sin ninguno, devuelve todos los proveedores paginados.
// Page y PageSize vienen de PageQuery. La búsqueda por texto se suma más adelante.
public class ProviderSearchQuery : PageQuery
{
    // ?type=Business. Si viene un valor que no es del enum, ASP.NET responde 400 solo.
    public ProviderType? Type { get; set; }
}
