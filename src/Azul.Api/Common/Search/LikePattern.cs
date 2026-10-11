namespace Azul.Api.Common.Search;

public static class LikePattern
{
    // Patrón para LIKE "contiene", en minúsculas y con %, _ y \ escapados.
    // Se usa junto con f_unaccent(lower(columna)) para buscar sin tildes ni mayúsculas.
    public static string Contains(string text)
        => $"%{Escape(text)}%".ToLowerInvariant();

    private static string Escape(string value)
        => value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
