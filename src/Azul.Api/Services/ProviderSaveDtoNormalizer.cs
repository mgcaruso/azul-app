using System.Globalization;
using Azul.Api.DTOs;

namespace Azul.Api.Services;

// Deja el request en la forma que espera el validador y que se guarda en la base.
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
        dto.PhoneNumber = NormalizePhoneNumber(dto.PhoneNumber);
        dto.InstagramUsername = NormalizeInstagram(dto.InstagramUsername);
    }

    // Lleva el celular a +549 + 10 dígitos (código de área sin 0 y número sin 15).
    // Acepta "2281 40-1234", "+54 9 2281 401234", "542281401234" o "02281 401234".
    // Si no puede, devuelve el texto limpio para que el validador lo rechace.
    public static string? NormalizePhoneNumber(string? value)
    {
        var cleaned = Clean(value);
        if (cleaned is null)
        {
            return null;
        }

        var digits = new string(cleaned.Where(char.IsAsciiDigit).ToArray());

        if (digits.StartsWith("549") && digits.Length == 13)
        {
            digits = digits[3..];
        }
        else if (digits.StartsWith("54") && digits.Length == 12)
        {
            digits = digits[2..];
        }
        else if (digits.StartsWith('0') && digits.Length == 11)
        {
            digits = digits[1..];
        }

        return digits.Length == 10 ? $"+549{digits}" : cleaned;
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
