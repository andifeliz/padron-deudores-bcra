using Microsoft.Extensions.Logging.Abstractions;
using PadronDeudores.Infraestructura.Archivos;
using PadronDeudores.Infraestructura.Persistencia;
using PadronDeudores.IntegracionTests.BaseDeDatos;

namespace PadronDeudores.IntegracionTests.Persistencia;

[Collection(ColeccionBaseDeDatos.Nombre)]
public class CargadorStagingTests(BaseDeDatosFixture fixture)
{
    [Fact]
    public async Task CargarAsync_DosVeces_ReemplazaElContenidoYGuardaElNivelDeRiesgo()
    {
        var cargador = new CargadorStaging(fixture.BaseDeDatos);

        await CargarMuestraAsync(cargador);
        var filas = await CargarMuestraAsync(cargador);

        Assert.Equal(4ul, filas);
        Assert.Equal(4L, await ConsultarAsync("SELECT count(*) FROM staging"));
        Assert.Equal(2L, await ConsultarAsync("SELECT count(*) FROM staging WHERE situacion = 5 AND nivel_riesgo = 5"));
    }

    private static async Task<ulong> CargarMuestraAsync(CargadorStaging cargador)
    {
        await using var archivo = File.OpenRead("Muestras/deudores-muestra.txt");
        var lector = new LectorDeudores(NullLogger<LectorDeudores>.Instance);
        return await cargador.CargarAsync(lector.LeerAsync(archivo, new EstadisticasLectura(), CancellationToken.None), CancellationToken.None);
    }

    private async Task<long> ConsultarAsync(string sql)
    {
        await using var comando = fixture.BaseDeDatos.CreateCommand(sql);
        return (long)(await comando.ExecuteScalarAsync())!;
    }
}
