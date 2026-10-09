using System.ComponentModel.DataAnnotations;

namespace Azul.Api.Entities;

public class Provider
{
    public int Id { get; set; }

    [MaxLength(100)]
    public required string Name { get; set; }

    public ProviderType Type { get; set; }

    public List<ProviderOffering> Offerings { get; set; } = [];
}
