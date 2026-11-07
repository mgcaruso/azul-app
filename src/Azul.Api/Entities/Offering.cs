using System.ComponentModel.DataAnnotations;

namespace Azul.Api.Entities;

public class Offering
{
    public int Id { get; set; }

    [MaxLength(100)]
    public required string Name { get; set; }

    // Other words people use to ask for this service (e.g. the trade instead of the discipline). Search matches them like the name.
    public List<string> Keywords { get; set; } = [];

    public required int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}