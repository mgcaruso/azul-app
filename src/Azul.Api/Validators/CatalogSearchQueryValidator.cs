using FluentValidation;
using Azul.Api.DTOs.Catalog;

namespace Azul.Api.Validators;

public class CatalogSearchQueryValidator : AbstractValidator<CatalogSearchQuery>
{
    public CatalogSearchQueryValidator()
    {
        Include(new PageQueryValidator());

        // Sin mínimo: la pantalla sin texto usa el mismo endpoint.
        RuleFor(x => x.Text)
            .MaximumLength(100).WithMessage("El texto de búsqueda no puede superar los 100 caracteres.");

        RuleFor(x => x.CategoryIds)
            .Must(ids => ids.Count <= 20).WithMessage("Se pueden elegir hasta 20 categorías.")
            .OverridePropertyName("category");

        RuleForEach(x => x.CategoryIds)
            .GreaterThan(0).WithMessage("La categoría no es válida.")
            .OverridePropertyName("category");
        
    }
}
