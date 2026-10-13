using System.Linq.Expressions;
using Azul.Api.Entities;

namespace Azul.Api.Common.Search;

// Qué es un perfil completo (foto, descripción e Instagram). Son Expression, y no métodos comunes,
// para que EF las traduzca a SQL dentro de un Where o un OrderBy.
public static class ProviderProfile
{
    // Puntaje de 0 a 3: suma 1 por foto, descripción e Instagram.
    public static readonly Expression<Func<Provider, int>> Score = p =>
        (!string.IsNullOrEmpty(p.PhotoKey) ? 1 : 0)
        + (!string.IsNullOrEmpty(p.Description) ? 1 : 0)
        + (!string.IsNullOrEmpty(p.InstagramUsername) ? 1 : 0);

    public static readonly Expression<Func<Provider, bool>> IsComplete = p =>
        !string.IsNullOrEmpty(p.PhotoKey)
        && !string.IsNullOrEmpty(p.Description)
        && !string.IsNullOrEmpty(p.InstagramUsername);
}
