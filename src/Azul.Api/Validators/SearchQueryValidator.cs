using Azul.Api.DTOs;
using FluentValidation;

namespace Azul.Api.Validators;

public class SearchQueryValidator : AbstractValidator<SearchQuery>
{
    public SearchQueryValidator()
    {
        RuleFor(x => x.Text)
            .Must(text => text.Trim().Length >= 2).WithMessage("Escribí al menos 2 letras.")
            .MaximumLength(100).WithMessage("El texto de búsqueda no puede superar los 100 caracteres.");
    }
}
