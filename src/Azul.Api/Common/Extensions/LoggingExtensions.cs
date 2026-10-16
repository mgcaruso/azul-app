using Serilog;

namespace Azul.Api.Common.Extensions;

public static class LoggingExtensions
{
    // Logs a consola y a archivo diario (logs/azul-AAAAMMDD.log), configurados en la sección Serilog de appsettings.
    // Si hay Seq:ServerUrl (en producción: variable de entorno Seq__ServerUrl) también se mandan a Seq.
    public static WebApplicationBuilder AddAppLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration);

            var seqUrl = context.Configuration["Seq:ServerUrl"];
            if (!string.IsNullOrWhiteSpace(seqUrl))
            {
                configuration.WriteTo.Seq(seqUrl);
            }
        });

        return builder;
    }
}
