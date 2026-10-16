using Azul.Api.Common.Search;
using Azul.Api.Entities;

namespace Azul.Api.Services.Providers;

public static class ProviderFilters
{
    public static IQueryable<Provider> WhereType(this IQueryable<Provider> providers, ProviderType? type)
    {
        return type is null ? providers : providers.Where(p => p.Type == type);
    }

    public static IQueryable<Provider> WhereInStatuses(this IQueryable<Provider> providers, List<ProviderStatus> statuses)
    {
        return statuses.Count == 0 ? providers : providers.Where(p => statuses.Contains(p.Status));
    }

    public static IQueryable<Provider> WhereInCategories(this IQueryable<Provider> providers, List<int> categoryIds)
    {
        if (categoryIds.Count > 0)
        {
            providers = providers
                .Where(p => p.Offerings.Any(po => categoryIds.Contains(po.Offering.CategoryId)));
        }
        return providers;
    }
    public static IQueryable<Provider> WhereMatchesText(this IQueryable<Provider> providers, string? text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            var pattern = LikePattern.Contains(text.Trim());
            // busco si el text coincide con nombre, descripcion del provider o con el nombre de alguno de los serivicos qeu brinda
            providers = providers.Where(p =>
                SearchText.Matches(p.DisplayName, pattern)
                || SearchText.Matches(p.Description, pattern)
                || p.Offerings.Any(po => SearchText.Matches(po.Offering.Name, pattern)));
        }

        return providers;
    }
}
