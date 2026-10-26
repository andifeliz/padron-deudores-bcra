using Dapper;
using PadronDeudores.IntegracionTests.Entorno;

namespace PadronDeudores.IntegracionTests.Procesamiento;

[Collection(ColeccionEntorno.Nombre)]
public class ProcesadorImportacionTests(EntornoFixture fixture)
{
    private const string Muestra = "Muestras/deudores-muestra.txt";

    [Fact]
    public async Task ProcesarAsync_ArchivoReal_CompletaLaImportacionConSuResumen()
    {
        var id = await fixture.RegistrarImportacionAsync(Muestra);

        var resumen = await fixture.CrearProcesador().ProcesarAsync(id, Muestra, CancellationToken.None);

        Assert.NotNull(resumen);
        Assert.Equal((5L, 1L, 4L, 2L), (resumen.LineasLeidas, resumen.LineasInvalidas, resumen.Deudores, resumen.Entidades));
        Assert.Equal("completada", await ConsultarAsync<string>("SELECT estado FROM importaciones WHERE id = @id", id));
        Assert.Equal(4L, await ConsultarAsync<long>("SELECT count(*) FROM deudores", id));
        Assert.Equal(0L, await ConsultarAsync<long>("SELECT count(*) FROM staging", id));
    }

    [Fact]
    public async Task ProcesarAsync_MensajeDuplicado_NoVuelveAProcesar()
    {
        var id = await fixture.RegistrarImportacionAsync(Muestra);
        var procesador = fixture.CrearProcesador();

        await procesador.ProcesarAsync(id, Muestra, CancellationToken.None);
        var segundaVez = await procesador.ProcesarAsync(id, Muestra, CancellationToken.None);

        Assert.Null(segundaVez);
    }

    private async Task<T> ConsultarAsync<T>(string sql, Guid id)
    {
        await using var conexion = await fixture.BaseDeDatos.OpenConnectionAsync();
        return await conexion.QuerySingleAsync<T>(sql, new { id });
    }
}
