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
}
