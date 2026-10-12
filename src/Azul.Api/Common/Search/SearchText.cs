namespace Azul.Api.Common.Search;

// Búsqueda de texto sin tildes ni mayúsculas, para usar dentro de un Where de EF:
//   providers.Where(p => SearchText.Matches(p.DisplayName, pattern))
// con pattern = LikePattern.Contains(texto). EF lo traduce a f_matches(columna, patrón) (migración
// FuncionBusquedaTexto). Si la columna es null da false, así que no hace falta chequearlo antes.
public static class SearchText
{
    public static bool Matches(string? value, string pattern)
        => throw new NotSupportedException("SearchText.Matches solo se puede usar en consultas de EF.");
}
