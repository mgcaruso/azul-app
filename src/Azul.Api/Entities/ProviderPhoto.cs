using System.ComponentModel.DataAnnotations;

namespace Azul.Api.Entities;

public class ProviderPhoto
{
    public int Id { get; set; }

    public int ProviderId { get; set; }
    public Provider Provider { get; set; } = null!;

    [MaxLength(200)]
    public required string Key { get; set; }

    public ProviderPhotoStatus Status { get; set; } = ProviderPhotoStatus.Pending;

    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
