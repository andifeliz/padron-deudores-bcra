using Dapper;
using Npgsql;

namespace PadronDeudores.Infraestructura.Persistencia;

// Consultas sobre la tabla entidades (solo lectura).
public sealed class RepositorioEntidades(NpgsqlDataSource baseDeDatos)
{
    public async Task<Entidad?> ObtenerAsync(string codigoEntidad, CancellationToken ct)
    {
        await using var conexion = await baseDeDatos.OpenConnectionAsync(ct);
        return await conexion.QuerySingleOrDefaultAsync<Entidad>(new CommandDefinition("""
            SELECT codigo_entidad AS CodigoEntidad, suma_total_prestamos AS SumaTotalPrestamos
            FROM entidades WHERE codigo_entidad = @codigoEntidad
            """, new { codigoEntidad }, cancellationToken: ct));
    }
}
