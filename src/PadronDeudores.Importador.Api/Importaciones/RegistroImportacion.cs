using PadronDeudores.Dominio;
using PadronDeudores.Infraestructura.Persistencia;

namespace PadronDeudores.Importador.Api.Importaciones;

// Último paso de las dos entradas (ruta local y upload): guarda la importación como pendiente y la manda a la cola.
// Si la cola falla, el pedido igual queda guardado: lo envía después el repartidor (patrón Outbox).
public sealed class RegistroImportacion(RepositorioImportaciones importaciones, EnvioImportaciones envio, ILogger<RegistroImportacion> logger)
{
    public async Task<IResult> RegistrarYEncolarAsync(Guid id, OrigenImportacion origen, string archivo, CancellationToken ct)
    {
        await importaciones.RegistrarAsync(id, origen, archivo, ct);

        try
        {
            await envio.EnviarAsync(id, archivo, ct);
            logger.LogInformation("Importación {ImportacionId} encolada: {Archivo}", id, archivo);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Importación {ImportacionId} guardada sin encolar: la enviará el repartidor", id);
        }

        return Results.Accepted($"/importaciones/{id}", new ImportacionAceptada(id, "pendiente"));
    }
}
