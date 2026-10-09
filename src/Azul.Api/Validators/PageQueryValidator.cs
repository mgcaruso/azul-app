using Azul.Api.Common.Pagination;
using FluentValidation;

namespace Azul.Api.Validators;

public class PageQueryValidator : AbstractValidator<PageQuery>
{
    public PageQueryValidator()
    {
        RuleFor(x => x.Page)
            .InclusiveBetween(1, 10_000).WithMessage("La página tiene que estar entre 1 y 10000.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50).WithMessage("El tamaño de página tiene que estar entre 1 y 50.");
    }
}
