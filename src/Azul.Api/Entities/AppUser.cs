using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Azul.Api.Entities;

public class AppUser : IdentityUser<Guid>
{
    [MaxLength(100)]
    public required string DisplayName { get; set; }

    [MaxLength(500)]
    public string? PictureUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public Provider? Provider { get; set; }   // 1 a 1, puede no tener ficha
}