using Azul.Api.DTOs;
using FluentValidation;

namespace Azul.Api.Validators;

public class OfferingSearchQueryValidator : AbstractValidator<OfferingSearchQuery>
{
    public OfferingSearchQueryValidator()
    {
        Include(new PageQueryValidator());

        RuleFor(x => x.Text)
            .MaximumLength(100).WithMessage("El texto de búsqueda no puede superar los 100 caracteres.");
    }
}
