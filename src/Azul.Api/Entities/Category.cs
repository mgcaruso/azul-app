using System.ComponentModel.DataAnnotations;

namespace Azul.Api.Entities;

public class Category
{
    public int Id { get; set; }
    [MaxLength(100)]
    public required string Name { get; set; }

    public List<Offering> Offerings { get; set; } = [];
}