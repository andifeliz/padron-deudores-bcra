using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace PadronDeudores.Infraestructura.Persistencia;

// Comprueba que la base responde, con la misma conexión y el mismo usuario que usa el servicio.
// Si falla o tarda demasiado, el servicio de health checks lo informa como Unhealthy.
public sealed class ChequeoBaseDeDatos(NpgsqlDataSource baseDeDatos) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        await using var consulta = baseDeDatos.CreateCommand("SELECT 1");
        await consulta.ExecuteScalarAsync(ct);
        return HealthCheckResult.Healthy();
    }
}
