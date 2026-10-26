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

    // Sin situación, el top de todos; con situación, solo los que tienen exactamente ese código.
    // Empate en la suma: primero la identificación más baja, para que el orden sea siempre el mismo.
    // Lo resuelve un índice (ix_deudores_top o ix_deudores_situacion_top), sin ordenar la tabla entera.
    public async Task<IReadOnlyList<Deudor>> ObtenerTopAsync(int cantidad, short? situacion, CancellationToken ct)
    {
        var filtro = situacion is null ? "" : "WHERE situacion_maxima = @situacion";
        await using var conexion = await baseDeDatos.OpenConnectionAsync(ct);
        var deudores = await conexion.QueryAsync<Deudor>(new CommandDefinition($"""
            SELECT nro_identificacion AS NroIdentificacion, situacion_maxima AS SituacionMaxima, suma_total_prestamos AS SumaTotalPrestamos
            FROM deudores
            {filtro}
            ORDER BY suma_total_prestamos DESC, nro_identificacion
            LIMIT @cantidad
            """, new { cantidad, situacion }, cancellationToken: ct));
        return deudores.AsList();
    }
}
