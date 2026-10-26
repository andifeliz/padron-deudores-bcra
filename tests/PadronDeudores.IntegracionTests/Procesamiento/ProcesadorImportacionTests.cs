using Dapper;
using PadronDeudores.Dominio;
using PadronDeudores.Infraestructura.Archivos;
using PadronDeudores.IntegracionTests.Entorno;
using PadronDeudores.IntegracionTests.Notificaciones;

namespace PadronDeudores.IntegracionTests.Procesamiento;

[Collection(ColeccionEntorno.Nombre)]
public class ProcesadorImportacionTests(EntornoFixture fixture)
{
    private const string Muestra = "Muestras/deudores-muestra.txt";

    [Fact]
    public async Task ProcesarAsync_ArchivoReal_CompletaLaImportacionConSuResumen()
    {
        var id = await fixture.RegistrarImportacionAsync(Muestra);
        var notificador = new NotificadorEnMemoria();

        var resumen = await fixture.CrearProcesador(notificador).ProcesarAsync(id, Muestra, CancellationToken.None);

        Assert.NotNull(resumen);
        Assert.Equal((5L, 1L, 4L, 2L), (resumen.LineasLeidas, resumen.LineasInvalidas, resumen.Deudores, resumen.Entidades));
        Assert.Equal("completada", await ConsultarAsync<string>("SELECT estado FROM importaciones WHERE id = @id", id));
        Assert.Equal(4L, await ConsultarAsync<long>("SELECT count(*) FROM deudores", id));
        Assert.Equal(0L, await ConsultarAsync<long>("SELECT count(*) FROM staging", id));
        Assert.Equal(EstadoImportacion.Completada, Assert.Single(notificador.Enviadas).Estado);
    }

    [Fact]
    public async Task ProcesarAsync_ArchivoEnS3_LoLeeDeAhiYCompletaLaImportacion()
    {
        await using var muestra = File.OpenRead(Muestra);
        var archivo = await new ArchivosS3(fixture.S3).SubirAsync(EntornoFixture.Bucket, "pruebas/muestra.txt", muestra, CancellationToken.None);
        var id = await fixture.RegistrarImportacionAsync(archivo);

        var resumen = await fixture.CrearProcesador().ProcesarAsync(id, archivo, CancellationToken.None);

        Assert.Equal((5L, 4L), (resumen!.LineasLeidas, resumen.Deudores));
    }

    [Fact]
    public async Task ProcesarAsync_ArchivoInexistente_MarcaFallidaSinAvisar()
    {
        var id = await fixture.RegistrarImportacionAsync("no-existe.txt");
        var notificador = new NotificadorEnMemoria();

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => fixture.CrearProcesador(notificador).ProcesarAsync(id, "no-existe.txt", CancellationToken.None));

        Assert.Equal("fallida", await ConsultarAsync<string>("SELECT estado FROM importaciones WHERE id = @id", id));
        // El aviso de falla lo da el consumidor, cuando ya no quedan reintentos.
        Assert.Empty(notificador.Enviadas);
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
