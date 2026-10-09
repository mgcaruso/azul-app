using Azul.Api.DTOs;
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
    }
}
