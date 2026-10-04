using System.ComponentModel.DataAnnotations;

namespace Azul.Api.DTOs;

public class CategorySaveDto
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
}
