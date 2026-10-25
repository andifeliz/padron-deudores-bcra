using Npgsql;
using NpgsqlTypes;
using PadronDeudores.Dominio;

namespace PadronDeudores.Infraestructura.Persistencia;

// Vacía la tabla staging y la llena con COPY binario, a medida que llegan las líneas
public sealed class CargadorStaging(NpgsqlDataSource baseDeDatos)
{
    private const string Copia =
        "COPY staging (codigo_entidad, nro_identificacion, situacion, nivel_riesgo, prestamos) FROM STDIN (FORMAT BINARY)";

    // Devuelve la cantidad de filas cargadas.
    public async Task<ulong> CargarAsync(IAsyncEnumerable<LineaDeudor> deudores, CancellationToken ct)
    {
        await using var conexion = await baseDeDatos.OpenConnectionAsync(ct);
        await using (var vaciar = new NpgsqlCommand("TRUNCATE staging", conexion))
        {
            await vaciar.ExecuteNonQueryAsync(ct);
        }

        await using var copia = await conexion.BeginBinaryImportAsync(Copia, ct);
        await foreach (var deudor in deudores.WithCancellation(ct))
        {
            await copia.StartRowAsync(ct);
            await copia.WriteAsync(deudor.CodigoEntidad, NpgsqlDbType.Varchar, ct);
            await copia.WriteAsync(deudor.NroIdentificacion, NpgsqlDbType.Varchar, ct);
            await copia.WriteAsync(deudor.Situacion, NpgsqlDbType.Smallint, ct);
            await EscribirNivelAsync(copia, NivelDeRiesgo.De(deudor.Situacion), ct);
            await copia.WriteAsync(deudor.Prestamos, NpgsqlDbType.Numeric, ct);
        }

        return await copia.CompleteAsync(ct);
    }

    // Un código no documentado no tiene nivel: se guarda null
    private static Task EscribirNivelAsync(NpgsqlBinaryImporter copia, short? nivel, CancellationToken ct) =>
        nivel is { } valor ? copia.WriteAsync(valor, NpgsqlDbType.Smallint, ct) : copia.WriteNullAsync(ct);
}
