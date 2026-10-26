using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;

namespace PadronDeudores.Infraestructura.Logs;

public static class ConfiguracionLogs
{
    // Logs estructurados en JSON por consola (un objeto por línea), con el nombre del servicio en cada uno.
    // El nivel mínimo se configura en la sección "Serilog" de appsettings.json.
    public static void AgregarLogsEstructurados(this IHostApplicationBuilder builder) =>
        builder.Services.AddSerilog(config => config
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Servicio", builder.Environment.ApplicationName)
            .WriteTo.Console(new RenderedCompactJsonFormatter()));
}
