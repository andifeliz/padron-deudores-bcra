using Microsoft.Extensions.Options;
using PadronDeudores.Importador.Worker.Notificaciones;
using PadronDeudores.Importador.Worker.Procesamiento;
using PadronDeudores.Infraestructura.Mensajeria;

namespace PadronDeudores.Importador.Worker;

// Lee los pedidos de importación de SQS, uno por vez, y los procesa.
public sealed class ConsumidorImportaciones(
    ColaImportaciones cola,
    ProcesadorImportacion procesador,
    INotificador notificador,
    IOptions<OpcionesAws> opciones,
    ILogger<ConsumidorImportaciones> logger) : BackgroundService
{
    // Mientras se procesa, se renueva el ocultamiento del mensaje para que otro worker no lo tome.
    private static readonly TimeSpan Visibilidad = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan IntervaloRenovacion = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan EsperaTrasError = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker iniciado");

        try
        {
            while (true)
            {
                await RecibirYAtenderAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker detenido");
        }
    }

    // Un error con un mensaje no detiene el worker: se loguea y se sigue con el próximo.
    private async Task RecibirYAtenderAsync(CancellationToken ct)
    {
        try
        {
            if (await cola.RecibirAsync(ct) is { } mensaje)
            {
                await AtenderAsync(mensaje, ct);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Ej.: la cola no responde. Se espera un poco para no reintentar sin pausa.
            logger.LogError(ex, "Error al leer la cola");
            await Task.Delay(EsperaTrasError, ct);
        }
    }

    private async Task AtenderAsync(MensajeRecibido recibido, CancellationToken ct)
    {
        // Si no se borra, SQS lo vuelve a entregar y, al tercer intento, lo pasa a la cola de fallidos.
        if (recibido.Contenido is not { } mensaje)
        {
            logger.LogError("Mensaje con formato inválido, intento {Intento}", recibido.Intento);
            return;
        }

        using var reloj = new PeriodicTimer(IntervaloRenovacion);
        var renovacion = RenovarVisibilidadAsync(reloj, recibido.Recibo, ct);

        try
        {
            await procesador.ProcesarAsync(mensaje.ImportacionId, mensaje.Archivo, ct);
            await cola.BorrarAsync(recibido.Recibo, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await InformarFallaAsync(mensaje, recibido.Intento, ex.Message);
        }
        finally
        {
            // Apagar el reloj termina la renovación.
            reloj.Dispose();
            await renovacion;
        }
    }

    // Solo se avisa "fallida" cuando no quedan reintentos: un intento intermedio puede salir bien después.
    private async Task InformarFallaAsync(MensajeImportacion mensaje, int intento, string error)
    {
        if (intento < opciones.Value.IntentosMaximos)
        {
            logger.LogWarning("Importación {ImportacionId}: falló el intento {Intento}, SQS la reintentará", mensaje.ImportacionId, intento);
            return;
        }

        logger.LogError("Importación {ImportacionId}: falló el último intento ({Intento}), pasa a la cola de fallidos", mensaje.ImportacionId, intento);
        await notificador.NotificarAsync(NotificacionImportacion.Fallida(mensaje.ImportacionId, mensaje.Archivo, error), CancellationToken.None);
    }

    private async Task RenovarVisibilidadAsync(PeriodicTimer reloj, string recibo, CancellationToken ct)
    {
        while (await reloj.WaitForNextTickAsync(ct))
        {
            try
            {
                await cola.ExtenderVisibilidadAsync(recibo, Visibilidad, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "No se pudo renovar la visibilidad del mensaje");
            }
        }
    }
}
