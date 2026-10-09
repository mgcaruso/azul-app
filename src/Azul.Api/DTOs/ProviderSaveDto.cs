using Azul.Api.Entities;

namespace Azul.Api.DTOs;

public class ProviderSaveDto
{
    public string Name { get; set; } = string.Empty;

    public ProviderType? Type { get; set; }

    public List<int> OfferingIds { get; set; } = [];
}
