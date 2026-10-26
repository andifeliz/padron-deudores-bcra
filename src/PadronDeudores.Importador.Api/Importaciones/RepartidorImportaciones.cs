using PadronDeudores.Infraestructura.Persistencia;

namespace PadronDeudores.Importador.Api.Importaciones;

// Repartidor del patrón Outbox: cada tanto busca importaciones guardadas cuyo mensaje no llegó a la cola
// (la cola estaba caída o la API se cortó justo después de guardar) y las envía.
public sealed class RepartidorImportaciones(
    RepositorioImportaciones importaciones,
    EnvioImportaciones envio,
    ILogger<RepartidorImportaciones> logger) : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(10);

    // Más que lo que tarda el envío normal de un pedido, para no mandar dos veces una que se está encolando.
    private static readonly TimeSpan Antiguedad = TimeSpan.FromSeconds(30);

    private const int MaximoPorVuelta = 100;

    // Al apagarse la API, la espera se cancela y el servicio termina.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var reloj = new PeriodicTimer(Intervalo);
        while (await reloj.WaitForNextTickAsync(stoppingToken))
        {
            await RepartirAsync(stoppingToken);
        }
    }

    // Una vuelta: envía las pendientes. Si una falla, sigue con las demás; se reintenta en la próxima vuelta.
    public async Task RepartirAsync(CancellationToken ct)
    {
        try
        {
            foreach (var importacion in await importaciones.ObtenerSinEncolarAsync(Antiguedad, MaximoPorVuelta, ct))
            {
                await EnviarAsync(importacion, ct);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "No se pudieron buscar las importaciones sin encolar");
        }
    }

    private async Task EnviarAsync(ImportacionSinEncolar importacion, CancellationToken ct)
    {
        try
        {
            await envio.EnviarAsync(importacion.Id, importacion.Archivo, ct);
            logger.LogInformation("Importación {ImportacionId} encolada por el repartidor", importacion.Id);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Importación {ImportacionId} sigue sin encolar: se reintenta en la próxima vuelta", importacion.Id);
        }
    }
}
