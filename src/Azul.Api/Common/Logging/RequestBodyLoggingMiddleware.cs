using Serilog;

namespace Azul.Api.Common.Logging;

// SOLO DESARROLLO: agrega el cuerpo JSON del pedido (POST/PUT/PATCH) al log de Serilog, para entender qué se mandó.
// No usar en producción: el cuerpo trae datos personales (celular, nombre).
public class RequestBodyLoggingMiddleware(RequestDelegate next, IDiagnosticContext diagnosticContext)
{
    private const int MaxBodyChars = 4000;

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var isJson = request.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) == true;
        var isWrite = HttpMethods.IsPost(request.Method) || HttpMethods.IsPut(request.Method) || HttpMethods.IsPatch(request.Method);
        var hasJsonBody = isJson && isWrite;

        if (hasJsonBody)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            diagnosticContext.Set("RequestBody", body.Length > MaxBodyChars ? body[..MaxBodyChars] : body);
        }

        await next(context);
    }
}
