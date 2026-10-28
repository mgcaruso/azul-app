using Azul.Api.DTOs.Providers;
using Azul.Api.Entities;

namespace Azul.Api.Services.Providers;

public static class ProviderSorting
{
    // Perfil más completo primero; después el más nuevo.
    public static IOrderedQueryable<Provider> SortByFeatured(this IQueryable<Provider> providers)
    {
        return providers
            .OrderByDescending(ProviderProfile.Score)
            .ThenByDescending(p => p.FirstPublishedAt)
            .ThenBy(p => p.Id);
    }

    // Siempre devuelve un orden total (desempata por Id) para que las páginas no se repitan ni se salteen.
    public static IOrderedQueryable<Provider> OrderBySort(this IQueryable<Provider> providers, ProviderSort providerSort)
    {
        switch (providerSort)
        {
            case ProviderSort.Newest:
                return providers.OrderByDescending(p => p.FirstPublishedAt).ThenBy(p => p.Id);

            case ProviderSort.Featured:
                return providers.SortByFeatured();

            default:
                throw new ArgumentOutOfRangeException(nameof(providerSort), providerSort, null);
        }
    }
}
