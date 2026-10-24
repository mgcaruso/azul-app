namespace Azul.Api.Services.Auth;

public record GoogleUser(string Subject, string Email, string Name, string? PictureUrl);