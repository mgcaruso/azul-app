using Azul.Api.DTOs.Offerings;
using FluentValidation;

namespace Azul.Api.Validators;

public class OfferingSaveDtoValidator : AbstractValidator<OfferingSaveDto>
{
    public OfferingSaveDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(x => x.CategoryId)
            .NotNull().WithMessage("La categoría es obligatoria.");

        RuleFor(x => x.Keywords)
            .Must(keywords => keywords is null || keywords.Count <= 20)
            .WithMessage("A service can have at most 20 keywords.");

        RuleForEach(x => x.Keywords)
            .MaximumLength(50).WithMessage("Each keyword can have at most 50 characters.");
    }
}
