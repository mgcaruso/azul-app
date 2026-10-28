namespace Azul.Api.DTOs.Auth;

public record MeDto(Guid Id, string Email, string DisplayName, string? PictureUrl, bool IsProvider);