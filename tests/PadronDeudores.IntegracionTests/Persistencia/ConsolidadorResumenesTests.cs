using Npgsql;
using PadronDeudores.Infraestructura.Persistencia;
using PadronDeudores.IntegracionTests.BaseDeDatos;

namespace PadronDeudores.IntegracionTests.Persistencia;

[Collection(ColeccionBaseDeDatos.Nombre)]
public class ConsolidadorResumenesTests(BaseDeDatosFixture fixture)
{
    [Fact]
    public async Task ReemplazarAsync_CalculaSituacionMaximaPorRiesgoYSumas()
    {
        await ImportarAsync("""
            ('00007', '20000000001',  5,    5, 100), ('00011', '20000000001', 11,    1,  50),
            ('00007', '20000000002',  1,    1,  20), ('00011', '20000000002', 11,    1,  30),
            ('00007', '20000000003', 99,    0,   0), ('00011', '20000000003',  1,    1,  10),
            ('00007', '20000000004', 11,    1,  80),
            ('00007', '20000000005',  2,    2,  10), ('00011', '20000000005',  7, null,  40)
            """);

        Assert.Equal(
            ["20000000001 5 150.0", "20000000002 1 50.0", "20000000003 1 10.0", "20000000004 11 80.0", "20000000005 2 50.0"],
            await ConsultarAsync("SELECT concat_ws(' ', nro_identificacion, situacion_maxima, suma_total_prestamos) FROM deudores ORDER BY 1"));
        Assert.Equal(
            ["00007 210.0", "00011 130.0"],
            await ConsultarAsync("SELECT concat_ws(' ', codigo_entidad, suma_total_prestamos) FROM entidades ORDER BY 1"));
    }

    [Fact]
    public async Task ReemplazarAsync_SegundaImportacion_QuitaAusentesYConservaLecturaDeConsulta()
    {
        await ImportarAsync("('00007', '20000000001', 1, 1, 10), ('00007', '20000000002', 1, 1, 20)");
        await ImportarAsync("('00007', '20000000002', 1, 1, 25)");

        Assert.Equal(["20000000002 25.0"], await ConsultarAsync(
            "SELECT concat_ws(' ', nro_identificacion, suma_total_prestamos) FROM deudores", rol: "consulta"));
    }

    // Carga staging con las filas dadas y ejecuta el reemplazo en una transacción, como lo hará el procesador.
    private async Task ImportarAsync(string filas)
    {
        await using var conexion = await fixture.BaseDeDatos.OpenConnectionAsync();
        await using (var staging = new NpgsqlCommand(
            $"TRUNCATE staging; INSERT INTO staging (codigo_entidad, nro_identificacion, situacion, nivel_riesgo, prestamos) VALUES {filas}", conexion))
        {
            await staging.ExecuteNonQueryAsync();
        }

        await using var transaccion = await conexion.BeginTransactionAsync();
        await new ConsolidadorResumenes().ReemplazarAsync(conexion, transaccion, CancellationToken.None);
        await transaccion.CommitAsync();
    }

    private async Task<List<string>> ConsultarAsync(string sql, string? rol = null)
    {
        await using var conexion = await fixture.BaseDeDatos.OpenConnectionAsync();
        await using var comando = new NpgsqlCommand(rol is null ? sql : $"SET ROLE {rol}; {sql}", conexion);
        await using var lector = await comando.ExecuteReaderAsync();

        var filas = new List<string>();
        while (await lector.ReadAsync())
        {
            filas.Add(lector.GetString(0));
        }

        return filas;
    }
}
