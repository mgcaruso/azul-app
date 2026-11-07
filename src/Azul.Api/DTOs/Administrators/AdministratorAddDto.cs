using System.ComponentModel.DataAnnotations;

namespace Azul.Api.DTOs.Administrators;

public class AdministratorAddDto
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;
}