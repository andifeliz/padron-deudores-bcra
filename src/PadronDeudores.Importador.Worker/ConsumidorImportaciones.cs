namespace PadronDeudores.Importador.Worker;

// Lee los pedidos de importación de SQS y los procesa. Por ahora solo arranca y espera.
public sealed class ConsumidorImportaciones(ILogger<ConsumidorImportaciones> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker iniciado");

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Worker detenido");
        }
    }
}
