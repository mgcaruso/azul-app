using Azul.Api.Common.Exceptions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Azul.Api.Common.Validation;

public static class ModelStateValidation
{
    private const string InvalidValueMessage = "El valor no es válido.";

    public static ValidationFailedException ToException(ModelStateDictionary modelState)
    {
        var errors = modelState
            .Where(entry => entry.Value is { Errors.Count: > 0 })
            .ToDictionary(
                entry => ValidationKeys.ToCamelCase(entry.Key),
                entry => entry.Value!.Errors
                    .Select(error => string.IsNullOrEmpty(error.ErrorMessage) ? InvalidValueMessage : error.ErrorMessage)
                    .ToArray());

        return new ValidationFailedException(errors);
    }

    public static void UseSpanishMessages(DefaultModelBindingMessageProvider messages)
    {
        messages.SetAttemptedValueIsInvalidAccessor((value, _) => $"El valor '{value}' no es válido.");
        messages.SetNonPropertyAttemptedValueIsInvalidAccessor(value => $"El valor '{value}' no es válido.");
        messages.SetValueIsInvalidAccessor(value => $"El valor '{value}' no es válido.");
        messages.SetUnknownValueIsInvalidAccessor(_ => InvalidValueMessage);
        messages.SetNonPropertyUnknownValueIsInvalidAccessor(() => InvalidValueMessage);
        messages.SetValueMustBeANumberAccessor(_ => "El valor tiene que ser un número.");
        messages.SetNonPropertyValueMustBeANumberAccessor(() => "El valor tiene que ser un número.");
        messages.SetValueMustNotBeNullAccessor(_ => "El valor es obligatorio.");
        messages.SetMissingBindRequiredValueAccessor(_ => "El valor es obligatorio.");
        messages.SetMissingKeyOrValueAccessor(() => "El valor es obligatorio.");
        messages.SetMissingRequestBodyRequiredValueAccessor(() => "Falta el body de la request.");
    }
}
