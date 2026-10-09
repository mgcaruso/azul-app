using Azul.Api.DTOs;
using FluentValidation;

namespace Azul.Api.Validators;

public class ProviderSaveDtoValidator : AbstractValidator<ProviderSaveDto>
{
    public ProviderSaveDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(x => x.Type)
            .NotNull().WithMessage("El tipo es obligatorio.")
            .IsInEnum().WithMessage("El tipo tiene que ser Individual o Business.");

        RuleFor(x => x.OfferingIds)
            .NotEmpty().WithMessage("Tiene que ofrecer al menos un servicio.");
    }
}
