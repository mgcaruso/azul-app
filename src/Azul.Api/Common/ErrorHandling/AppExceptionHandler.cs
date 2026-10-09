using Azul.Api.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.Common.ErrorHandling;

public class AppExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<AppExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var status = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            ValidationFailedException => StatusCodes.Status400BadRequest,
            BusinessRuleException => StatusCodes.Status422UnprocessableEntity,
            ForbiddenException => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new ProblemDetails { Status = status };

        if (exception is AppException appException)
        {
            problem.Detail = appException.Message;
            problem.Extensions["code"] = appException.Code;
        }
        else
        {
            logger.LogError(exception, "Error inesperado en {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
            problem.Detail = "Ocurrió un error inesperado.";
            problem.Extensions["code"] = "server.unexpected";
        }

        if (exception is NotFoundException notFound)
        {
            problem.Extensions["resource"] = notFound.Resource;
            problem.Extensions["id"] = notFound.Id;
        }

        if (exception is ConflictException { Field: not null } conflict)
        {
            problem.Extensions["field"] = conflict.Field;
        }

        if (exception is ValidationFailedException validation)
        {
            problem.Extensions["errors"] = validation.Errors;
        }

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem
        });
    }
}
