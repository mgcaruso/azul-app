using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;

namespace Azul.Api.Common.Logging;

// Formato de consola para producción: una línea JSON por evento, que Cloud Logging (Cloud Run) entiende solo.
// "severity" le dice el nivel (para filtrar errores) y cada propiedad del log (ClientIp, StatusCode, RequestPath...)
// queda como un campo de jsonPayload, filtrable en el Explorador de registros.
public sealed class GoogleCloudJsonFormatter : ITextFormatter
{
    private static readonly JsonValueFormatter ValueFormatter = new(typeTagName: null);

    // Campos que escribe el formatter; una propiedad del log con el mismo nombre se saltea para no duplicarlos.
    private static readonly HashSet<string> ReservedNames = ["severity", "message", "time", "messageTemplate"];

    public void Format(LogEvent logEvent, TextWriter output)
    {
        output.Write("{\"severity\":");
        JsonValueFormatter.WriteQuotedJsonString(ToSeverity(logEvent.Level), output);

        var message = logEvent.RenderMessage();
        if (logEvent.Exception is not null)
        {
            // Con el stack trace dentro del mensaje, Error Reporting de Google agrupa los errores solo.
            message += Environment.NewLine + logEvent.Exception;
        }
        output.Write(",\"message\":");
        JsonValueFormatter.WriteQuotedJsonString(message, output);

        output.Write(",\"time\":");
        JsonValueFormatter.WriteQuotedJsonString(logEvent.Timestamp.UtcDateTime.ToString("O"), output);

        // La plantilla sin valores sirve para agrupar ("todos los 'HTTP ... responded ...'").
        output.Write(",\"messageTemplate\":");
        JsonValueFormatter.WriteQuotedJsonString(logEvent.MessageTemplate.Text, output);

        foreach (var (name, value) in logEvent.Properties)
        {
            if (ReservedNames.Contains(name))
            {
                continue;
            }

            output.Write(',');
            JsonValueFormatter.WriteQuotedJsonString(name, output);
            output.Write(':');
            ValueFormatter.Format(value, output);
        }

        output.WriteLine('}');
    }

    // Niveles de Serilog a los de Cloud Logging.
    private static string ToSeverity(LogEventLevel level) => level switch
    {
        LogEventLevel.Verbose or LogEventLevel.Debug => "DEBUG",
        LogEventLevel.Information => "INFO",
        LogEventLevel.Warning => "WARNING",
        LogEventLevel.Error => "ERROR",
        _ => "CRITICAL",
    };
}
