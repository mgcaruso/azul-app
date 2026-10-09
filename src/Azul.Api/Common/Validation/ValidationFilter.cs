using Azul.Api.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Azul.Api.Common.Validation;

public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new Dictionary<string, List<string>>();

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var result = await validator.ValidateAsync(
                new ValidationContext<object>(argument), context.HttpContext.RequestAborted);

            foreach (var failure in result.Errors)
            {
                var key = ValidationKeys.ToCamelCase(failure.PropertyName);
                if (!errors.TryGetValue(key, out var messages))
                {
                    errors[key] = messages = [];
                }
                messages.Add(failure.ErrorMessage);
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationFailedException(errors.ToDictionary(e => e.Key, e => e.Value.ToArray()));
        }

        await next();
    }
}
