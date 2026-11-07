using System.Globalization;
using Azul.Api.DTOs.Providers;

namespace Azul.Api.Services.Providers;

// Deja el request en la forma que espera el validador y que se guarda en la base (limpieza, no corrección).
// Es idempotente: el validador lo corre antes de validar y el service otra vez antes de guardar.
public static class ProviderSaveDtoNormalizer
{
    public static void Normalize(ProviderSaveDto dto)
    {
        dto.FirstName = NormalizeName(dto.FirstName);
        dto.LastName = NormalizeName(dto.LastName);
        dto.DisplayName = NormalizeName(dto.DisplayName);
        dto.Description = Clean(dto.Description);
        dto.PhotoKey = Clean(dto.PhotoKey);
        dto.Address = Clean(dto.Address);
        dto.Hours = Clean(dto.Hours);
        dto.PhoneNumber = NormalizePhone(dto.PhoneNumber);
        dto.CallPhone = NormalizePhone(dto.CallPhone);
        dto.ExtraUrl = Clean(dto.ExtraUrl);
        dto.InstagramUsername = NormalizeInstagram(dto.InstagramUsername);
    }

    // No corrige números: solo quita los adornos (espacios, guiones, puntos y paréntesis) para que
    // "+54 9 2281 40-1234" y "+5492281401234" valgan lo mismo. Si el número está mal escrito
    // (sin "+", sin código de país, con letras) lo deja como está y el validador lo rechaza con un mensaje claro.
    public static string? NormalizePhone(string? value)
    {
        var cleaned = Clean(value);
        return cleaned is null
            ? null
            : new string(cleaned.Where(c => c is not (' ' or '-' or '.' or '(' or ')')).ToArray());
    }

    public static string? NormalizeInstagram(string? value)
    {
        var cleaned = Clean(value)?.TrimStart('@');
        return string.IsNullOrEmpty(cleaned) ? null : cleaned.ToLowerInvariant();
    }

    // Nombre, apellido o marca: solo la primera letra en mayúscula; el resto queda como lo escribieron.
    // "josefina" → "Josefina", "tecnoFix azul" → "TecnoFix azul".
    public static string? NormalizeName(string? value)
    {
        var cleaned = Clean(value);
        return cleaned is null ? null : char.ToUpper(cleaned[0], SpanishCulture) + cleaned[1..];
    }

    private static readonly CultureInfo SpanishCulture = CultureInfo.GetCultureInfo("es-AR");

    private static string? Clean(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
