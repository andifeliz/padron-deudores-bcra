using System.Diagnostics;
using System.Text.Json;
using Dapper;
using Npgsql;
using PadronDeudores.Dominio;
using PadronDeudores.Infraestructura.Archivos;
using PadronDeudores.Infraestructura.Persistencia;

namespace PadronDeudores.Importador.Worker.Procesamiento;

// Coordina una importación: lock, estado, lectura, carga en staging, reemplazo de resúmenes y resumen final.
public sealed class ProcesadorImportacion(
    NpgsqlDataSource baseDeDatos,
    IOrigenArchivo origen,
    LectorDeudores lector,
    CargadorStaging cargador,
    ConsolidadorResumenes consolidador,
    RepositorioImportaciones importaciones,
    ILogger<ProcesadorImportacion> logger)
{
    // Una sola importación a la vez, aunque haya varios workers. PostgreSQL libera el lock si el worker se cae.
    private const long ClaveLock = 20231100;

    private static readonly JsonSerializerOptions FormatoJson = new(JsonSerializerDefaults.Web);

    // Devuelve null si la importación no existe o ya estaba completada (mensaje duplicado).
    public async Task<ResumenImportacion?> ProcesarAsync(Guid id, string archivo, CancellationToken ct)
    {
        await using var conexionLock = await baseDeDatos.OpenConnectionAsync(ct);
        await conexionLock.ExecuteAsync(new CommandDefinition("SELECT pg_advisory_lock(@ClaveLock)", new { ClaveLock }, cancellationToken: ct));

        try
        {
            switch (await importaciones.ObtenerEstadoAsync(id, ct))
            {
                case null:
                    logger.LogWarning("Importación {ImportacionId} inexistente: se ignora el mensaje", id);
                    return null;
                case EstadoImportacion.Completada:
                    logger.LogInformation("Importación {ImportacionId} ya completada: se ignora el mensaje duplicado", id);
                    return null;
            }

            return await ImportarAsync(id, archivo, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Importación {ImportacionId} fallida", id);
            await importaciones.MarcarFallidaAsync(id, ex.Message, CancellationToken.None);
            throw;
        }
        finally
        {
            // La conexión vuelve al pool sin cerrarse: si no se libera el lock acá, la próxima importación esperaría para siempre.
            await conexionLock.ExecuteAsync("SELECT pg_advisory_unlock(@ClaveLock)", new { ClaveLock });
        }
    }

    private async Task<ResumenImportacion> ImportarAsync(Guid id, string archivo, CancellationToken ct)
    {
        // Todos los logs de esta importación (también los del lector) llevan su id.
        using var alcance = logger.BeginScope(new Dictionary<string, object> { ["ImportacionId"] = id });

        await importaciones.MarcarProcesandoAsync(id, ct);
        logger.LogInformation("Importación {ImportacionId} iniciada: {Archivo}", id, archivo);
        var reloj = Stopwatch.StartNew();
        var estadisticas = new EstadisticasLectura();

        await using (var stream = await origen.AbrirAsync(archivo, ct))
        {
            await cargador.CargarAsync(lector.LeerAsync(stream, estadisticas, ct), ct);
        }

        logger.LogInformation("Archivo leído: {Lineas} líneas en {Segundos} s. Calculando resúmenes",
            estadisticas.LineasLeidas, Math.Round(reloj.Elapsed.TotalSeconds, 1));

        await using var conexion = await baseDeDatos.OpenConnectionAsync(ct);
        await using var transaccion = await conexion.BeginTransactionAsync(ct);
        await consolidador.ReemplazarAsync(conexion, transaccion, ct);

        var resumen = await ArmarResumenAsync(conexion, transaccion, id, estadisticas, reloj, ct);
        await cargador.VaciarAsync(conexion, transaccion, ct);
        await importaciones.MarcarCompletadaAsync(conexion, transaccion, id, JsonSerializer.Serialize(resumen, FormatoJson), ct);
        await transaccion.CommitAsync(ct);

        logger.LogInformation("Importación {ImportacionId} completada {@Resumen}", id, resumen);
        return resumen;
    }

    private static async Task<ResumenImportacion> ArmarResumenAsync(
        NpgsqlConnection conexion, NpgsqlTransaction transaccion, Guid id, EstadisticasLectura estadisticas, Stopwatch reloj, CancellationToken ct)
    {
        var totales = await conexion.QuerySingleAsync<(long Deudores, long Entidades)>(new CommandDefinition(
            "SELECT (SELECT count(*) FROM deudores), (SELECT count(*) FROM entidades)", transaction: transaccion, cancellationToken: ct));

        // Códigos de situación sin nivel de riesgo (no documentados): se informan para revisarlos.
        var desconocidos = await conexion.QueryAsync<(short Codigo, long Lineas)>(new CommandDefinition(
            "SELECT situacion, count(*) FROM staging WHERE nivel_riesgo IS NULL GROUP BY situacion", transaction: transaccion, cancellationToken: ct));

        return new ResumenImportacion(
            id,
            estadisticas.LineasLeidas,
            estadisticas.LineasInvalidas,
            totales.Deudores,
            totales.Entidades,
            estadisticas.LineasPorPeriodo,
            desconocidos.ToDictionary(d => d.Codigo, d => d.Lineas),
            Math.Round(reloj.Elapsed.TotalSeconds, 1));
    }
}
