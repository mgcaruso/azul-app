using Azul.Api.DTOs.Auth;
using FluentValidation;

namespace Azul.Api.Validators;

public class GoogleSignInDtoValidator : AbstractValidator<GoogleSignInDto>
{
    public GoogleSignInDtoValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Falta el token de Google.")
            .MaximumLength(4096).WithMessage("El token de Google es demasiado largo.");
    }
}
