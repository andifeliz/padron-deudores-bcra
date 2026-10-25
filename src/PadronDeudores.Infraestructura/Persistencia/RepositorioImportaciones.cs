using Dapper;
using Npgsql;
using PadronDeudores.Dominio;

namespace PadronDeudores.Infraestructura.Persistencia;

// Lee y actualiza el estado de las importaciones.
public sealed class RepositorioImportaciones(NpgsqlDataSource baseDeDatos)
{
    public async Task<EstadoImportacion?> ObtenerEstadoAsync(Guid id, CancellationToken ct)
    {
        await using var conexion = await baseDeDatos.OpenConnectionAsync(ct);
        var estado = await conexion.QuerySingleOrDefaultAsync<string>(
            new CommandDefinition("SELECT estado FROM importaciones WHERE id = @id", new { id }, cancellationToken: ct));
        return estado is null ? null : Enum.Parse<EstadoImportacion>(estado, ignoreCase: true);
    }

    public Task MarcarProcesandoAsync(Guid id, CancellationToken ct) =>
        EjecutarAsync("UPDATE importaciones SET estado = @estado, iniciada_en = now() WHERE id = @id",
            new { id, estado = Texto(EstadoImportacion.Procesando) }, ct);

    public Task MarcarFallidaAsync(Guid id, string error, CancellationToken ct) =>
        EjecutarAsync("UPDATE importaciones SET estado = @estado, finalizada_en = now(), error = @error WHERE id = @id",
            new { id, error, estado = Texto(EstadoImportacion.Fallida) }, ct);

    // Va en la transacción del reemplazo: los datos nuevos y el estado "completada" se confirman juntos.
    public Task MarcarCompletadaAsync(NpgsqlConnection conexion, NpgsqlTransaction transaccion, Guid id, string resumen, CancellationToken ct) =>
        conexion.ExecuteAsync(new CommandDefinition(
            "UPDATE importaciones SET estado = @estado, finalizada_en = now(), resumen = @resumen::jsonb, error = null WHERE id = @id",
            new { id, resumen, estado = Texto(EstadoImportacion.Completada) }, transaccion, cancellationToken: ct));

    private async Task EjecutarAsync(string sql, object parametros, CancellationToken ct)
    {
        await using var conexion = await baseDeDatos.OpenConnectionAsync(ct);
        await conexion.ExecuteAsync(new CommandDefinition(sql, parametros, cancellationToken: ct));
    }

    private static string Texto(EstadoImportacion estado) => estado.ToString().ToLowerInvariant();
}
