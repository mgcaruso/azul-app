using System.Text.RegularExpressions;
using Azul.Api.DTOs;
using Azul.Api.Entities;
using Azul.Api.Services;
using FluentValidation;
using FluentValidation.Results;

namespace Azul.Api.Validators;

public partial class ProviderSaveDtoValidator : AbstractValidator<ProviderSaveDto>
{
    [GeneratedRegex(@"[\p{So}\p{Cs}]")] private static partial Regex EmojiRegex();
    [GeneratedRegex(@"^\+549\d{10}$")] private static partial Regex PhoneRegex();
    [GeneratedRegex(@"^[a-z0-9._]+$")] private static partial Regex InstagramRegex();

    public ProviderSaveDtoValidator()
    {
        RuleFor(x => x.Type)
            .NotNull().WithMessage("El tipo es obligatorio.")
            .IsInEnum().WithMessage("El tipo tiene que ser Individual, Venture o Business.");

        // Individual: nombre y apellido
        When(x => x.Type == ProviderType.Individual, () =>
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(50).WithMessage("El nombre no puede superar los 50 caracteres.")
                .Must(NoEmoji).WithMessage("El nombre no puede tener emojis.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("El apellido es obligatorio.")
                .MaximumLength(50).WithMessage("El apellido no puede superar los 50 caracteres.")
                .Must(NoEmoji).WithMessage("El apellido no puede tener emojis.");
        });

        // Emprendimiento y Negocio: solo la marca
        When(x => x.Type is ProviderType.Venture or ProviderType.Business, () =>
        {
            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.")
                .Must(NoEmoji).WithMessage("El nombre no puede tener emojis.");

            RuleFor(x => x.FirstName).Empty().WithMessage("Solo una persona lleva nombre y apellido.");
            RuleFor(x => x.LastName).Empty().WithMessage("Solo una persona lleva nombre y apellido.");
        });

        // Negocio con local: dirección obligatoria
        When(x => x.Type == ProviderType.Business, () =>
            RuleFor(x => x.Address).NotEmpty().WithMessage("La dirección es obligatoria para un negocio con local."));

        RuleFor(x => x.Address).MaximumLength(150).WithMessage("La dirección no puede superar los 150 caracteres.");
        RuleFor(x => x.Hours).MaximumLength(200).WithMessage("Los horarios no pueden superar los 200 caracteres.");
        RuleFor(x => x.Description).MaximumLength(500).WithMessage("La descripción no puede superar los 500 caracteres.");
        RuleFor(x => x.PhotoKey).MaximumLength(200).WithMessage("La foto no puede superar los 200 caracteres.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("El celular es obligatorio.")
            .Matches(PhoneRegex()).WithMessage("El celular tiene que ser un número argentino, por ejemplo 2281 40-1234.");

        RuleFor(x => x.InstagramUsername)
            .MaximumLength(30).WithMessage("El usuario de Instagram no puede superar los 30 caracteres.")
            .Matches(InstagramRegex()).WithMessage("El usuario de Instagram solo puede tener letras, números, puntos y guiones bajos.")
            .When(x => !string.IsNullOrEmpty(x.InstagramUsername));

        RuleFor(x => x.AcceptedTerms)
            .Equal(true).WithMessage("Tenés que aceptar que publiquemos tus datos.");

        RuleFor(x => x.OfferingIds)
            .NotEmpty().WithMessage("Tiene que ofrecer al menos un servicio.")
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Hay servicios repetidos.");
    }

    // Normaliza antes de validar (celular a +549…, Instagram sin @ y en minúsculas, Trim()),
    // así las reglas ven lo mismo que después se guarda.
    protected override bool PreValidate(ValidationContext<ProviderSaveDto> context, ValidationResult result)
    {
        ProviderSaveDtoNormalizer.Normalize(context.InstanceToValidate);
        return true;
    }

    private static bool NoEmoji(string? value) => value is null || !EmojiRegex().IsMatch(value);
}
