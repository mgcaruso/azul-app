using System.Linq.Expressions;
using Azul.Api.DTOs.Offerings;
using Azul.Api.DTOs.Providers;
using Azul.Api.Entities;

namespace Azul.Api.Services.Providers;

// Cómo se convierte un Provider en cada DTO, para usar dentro de un Select de EF.
// Son métodos que devuelven una Expression (y no métodos que reciben un Provider) para que EF
// pueda leer la fórmula y traducirla a SQL. photoBaseUrl llega como parámetro porque sale de la
// configuración; EF lo manda como un valor más de la consulta.
public static class ProviderProjections
{
    public static Expression<Func<Provider, ProviderSummaryDto>> ToSummaryDto(string photoBaseUrl) =>
        p => new ProviderSummaryDto
        {
            Id = p.Id,
            DisplayName = p.DisplayName,
            Type = p.Type,
            PhotoUrl = p.PhotoKey == null ? null : photoBaseUrl + p.PhotoKey,
            Description = p.Description,
            PhoneNumber = p.PhoneNumber,
            InstagramUsername = p.InstagramUsername,
            CategoryName = p.Offerings
                .OrderBy(po => po.Offering.Name)
                .Select(po => po.Offering.Category.Name)
                .FirstOrDefault(),
            TopOfferings = p.Offerings
                .OrderBy(po => po.Offering.Name)
                .Take(2)
                .Select(po => new OfferingSummaryDto { Id = po.Offering.Id, Name = po.Offering.Name })
                .ToList(),
            OfferingCount = p.Offerings.Count
        };

    public static Expression<Func<Provider, ProviderDto>> ToDto(string photoBaseUrl) =>
        p => new ProviderDto
        {
            Id = p.Id,
            DisplayName = p.DisplayName,
            Type = p.Type,
            Description = p.Description,
            PhotoUrl = p.PhotoKey == null ? null : photoBaseUrl + p.PhotoKey,
            PhoneNumber = p.PhoneNumber,
            InstagramUsername = p.InstagramUsername,
            Address = p.Address,
            Hours = p.Hours,
            Offerings = p.Offerings
                .OrderBy(po => po.Offering.Name)
                .Select(po => new OfferingSummaryDto { Id = po.Offering.Id, Name = po.Offering.Name })
                .ToList()
        };

    public static Expression<Func<Provider, AdminProviderDto>> ToAdminDto(string photoBaseUrl) =>
        p => new AdminProviderDto
        {
            Id = p.Id,
            DisplayName = p.DisplayName,
            Type = p.Type,
            Description = p.Description,
            PhotoUrl = p.PhotoKey == null ? null : photoBaseUrl + p.PhotoKey,
            PhoneNumber = p.PhoneNumber,
            InstagramUsername = p.InstagramUsername,
            Address = p.Address,
            Hours = p.Hours,
            Offerings = p.Offerings
                .OrderBy(po => po.Offering.Name)
                .Select(po => new OfferingSummaryDto { Id = po.Offering.Id, Name = po.Offering.Name })
                .ToList(),
            FirstName = p.FirstName,
            LastName = p.LastName,
            PhotoKey = p.PhotoKey,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            FirstPublishedAt = p.FirstPublishedAt
        };

    public static Expression<Func<Provider, AdminProviderListItemDto>> ToAdminListItemDto() =>
        p => new AdminProviderListItemDto
        {
            Id = p.Id,
            DisplayName = p.DisplayName,
            Type = p.Type,
            PhoneNumber = p.PhoneNumber,
            Status = p.Status,
            CreatedAt = p.CreatedAt
        };
}
