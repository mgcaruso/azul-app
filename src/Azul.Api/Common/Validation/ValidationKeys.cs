namespace Azul.Api.Common.Validation;

public static class ValidationKeys
{
    public static string ToCamelCase(string key)
    {
        key = key.TrimStart('$').TrimStart('.');

        return string.Join('.', key.Split('.')
            .Select(segment => segment.Length == 0 ? segment : char.ToLowerInvariant(segment[0]) + segment[1..]));
    }
}
