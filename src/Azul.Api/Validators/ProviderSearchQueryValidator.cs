using Azul.Api.DTOs;
using FluentValidation;

namespace Azul.Api.Validators;

public class ProviderSearchQueryValidator : AbstractValidator<ProviderSearchQuery>
{
    public ProviderSearchQueryValidator()
    {
        Include(new PageQueryValidator());
    }
}
