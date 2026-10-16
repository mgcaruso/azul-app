using Azul.Api.Entities;

namespace Azul.Api.DTOs.Providers;

public class AdminProviderDto : ProviderDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhotoKey { get; set; }

    public ProviderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FirstPublishedAt { get; set; }

    public List<DuplicateCandidateDto> PossibleDuplicates { get; set; } = [];
}
