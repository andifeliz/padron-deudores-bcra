using Dapper;
using Npgsql;

namespace PadronDeudores.Infraestructura.Persistencia;

// Consultas sobre la tabla deudores (solo lectura).
public sealed class RepositorioDeudores(NpgsqlDataSource baseDeDatos)
{
    public async Task<Deudor?> ObtenerAsync(string nroIdentificacion, CancellationToken ct)
    {
        await using var conexion = await baseDeDatos.OpenConnectionAsync(ct);
        return await conexion.QuerySingleOrDefaultAsync<Deudor>(new CommandDefinition("""
            SELECT nro_identificacion AS NroIdentificacion, situacion_maxima AS SituacionMaxima, suma_total_prestamos AS SumaTotalPrestamos
            FROM deudores WHERE nro_identificacion = @nroIdentificacion
            """, new { nroIdentificacion }, cancellationToken: ct));
    }

    // Empate en la suma: primero la identificación más baja, para que el orden sea siempre el mismo.
    // Lo resuelve el índice ix_deudores_top, sin ordenar la tabla entera.
    public async Task<IReadOnlyList<Deudor>> ObtenerTopAsync(int cantidad, CancellationToken ct)
    {
        await using var conexion = await baseDeDatos.OpenConnectionAsync(ct);
        var deudores = await conexion.QueryAsync<Deudor>(new CommandDefinition("""
            SELECT nro_identificacion AS NroIdentificacion, situacion_maxima AS SituacionMaxima, suma_total_prestamos AS SumaTotalPrestamos
            FROM deudores
            ORDER BY suma_total_prestamos DESC, nro_identificacion
            LIMIT @cantidad
            """, new { cantidad }, cancellationToken: ct));
        return deudores.AsList();
    }
}
