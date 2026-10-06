using System.ComponentModel.DataAnnotations;

namespace Azul.Api.Entities;

public class Offering
{
    public int Id { get; set; }

    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(300)]
    public string? Description { get; set; }

    public required int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}