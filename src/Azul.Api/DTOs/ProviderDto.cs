using Azul.Api.Entities;

namespace Azul.Api.DTOs;

public class ProviderDto
{
    public int Id { get; set; }
    public required string DisplayName { get; set; }
    public ProviderType Type { get; set; }
    public string? Description { get; set; }
    public string? PhotoUrl { get; set; }
    public required string PhoneNumber { get; set; }
    public string? InstagramUsername { get; set; }
    public string? Address { get; set; }
    public string? Hours { get; set; }
    public List<OfferingSummaryDto> Offerings { get; set; } = [];
}
