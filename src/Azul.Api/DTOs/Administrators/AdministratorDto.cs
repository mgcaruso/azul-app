namespace Azul.Api.DTOs.Administrators;

public record AdministratorDto(Guid Id, string Email, string DisplayName, string? PictureUrl);