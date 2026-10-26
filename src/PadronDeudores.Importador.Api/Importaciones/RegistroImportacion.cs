using PadronDeudores.Dominio;
using PadronDeudores.Infraestructura.Mensajeria;
using PadronDeudores.Infraestructura.Persistencia;

namespace PadronDeudores.Importador.Api.Importaciones;

// Último paso de las dos entradas (ruta local y upload): registra la importación como pendiente y la encola.
public sealed class RegistroImportacion(RepositorioImportaciones importaciones, ColaImportaciones cola, ILogger<RegistroImportacion> logger)
{
    public async Task<IResult> RegistrarYEncolarAsync(Guid id, OrigenImportacion origen, string archivo, CancellationToken ct)
    {
        await importaciones.RegistrarAsync(id, origen, archivo, ct);

        try
        {
            await cola.EncolarAsync(new MensajeImportacion(id, archivo), ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "No se pudo encolar la importación {ImportacionId}", id);
            await importaciones.MarcarFallidaAsync(id, "No se pudo encolar el pedido.", CancellationToken.None);
            return Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Cola no disponible");
        }

        logger.LogInformation("Importación {ImportacionId} encolada: {Archivo}", id, archivo);
        return Results.Accepted($"/importaciones/{id}", new ImportacionAceptada(id, "pendiente"));
    }
}
