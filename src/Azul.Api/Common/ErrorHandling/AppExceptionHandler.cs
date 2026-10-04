using Azul.Api.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Azul.Api.Common.ErrorHandling;

// Único lugar que traduce excepciones a HTTP.
// Lo llama UseExceptionHandler() cuando una excepción sale sin que nadie la atrape.
public class AppExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<AppExceptionHandler> logger) : IExceptionHandler
{
    // El CancellationToken lo exige la interfaz: solo se pasa.
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
            // Inesperado: se registra completo en el log, pero al cliente no le llega nada interno.
            logger.LogError(exception, "Error inesperado en {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
            problem.Detail = "Ocurrió un error inesperado.";
            problem.Extensions["code"] = "server.unexpected";
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

        // IProblemDetailsService (registrado por AddProblemDetails) completa type, title y traceId,
        // así el formato es el mismo que el de los 400 de validación de ASP.NET.
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem
        });
    }
}
