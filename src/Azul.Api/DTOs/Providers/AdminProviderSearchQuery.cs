using Azul.Api.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.DTOs.Providers;

// GET /api/admin/providers?status=&status=&text=&type=&page=&pageSize=
public class AdminProviderSearchQuery : ProviderSearchQuery
{
    public string? Text { get; set; }

    // Se repite en la URL (?status=Pending&status=Suspended). Entre estados es "o"; vacío = todos.
    [FromQuery(Name = "status")]
    public List<ProviderStatus> Statuses { get; set; } = [];
}
