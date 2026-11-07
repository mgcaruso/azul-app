using Azul.Api.Common.Logging;
using Serilog;

namespace Azul.Api.Common.Extensions;

public static class LoggingExtensions
{
    // Niveles y enrichers salen de la sección Serilog de appsettings; los destinos se eligen acá según el entorno.
    // Si hay Seq:ServerUrl (en producción: variable de entorno Seq__ServerUrl) también se mandan a Seq.
    public static WebApplicationBuilder AddAppLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration);

            if (context.HostingEnvironment.IsDevelopment())
            {
                // Consola legible y archivo diario (logs/azul-AAAAMMDD.log), se guardan 14 días.
                configuration.WriteTo.Console();
                configuration.WriteTo.File("logs/azul-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14);
            }
            else
            {
                // Cloud Run manda la consola a Cloud Logging: una línea JSON por evento, con severity y propiedades filtrables.
                // Sin archivo: el disco de Cloud Run es temporal y vive en memoria.
                configuration.WriteTo.Console(new GoogleCloudJsonFormatter());
            }

            var seqUrl = context.Configuration["Seq:ServerUrl"];
            if (!string.IsNullOrWhiteSpace(seqUrl))
            {
                configuration.WriteTo.Seq(seqUrl);
            }
        });

        return builder;
    }
}
