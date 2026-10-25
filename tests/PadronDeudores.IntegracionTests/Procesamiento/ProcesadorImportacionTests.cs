using Dapper;
using Microsoft.Extensions.Logging.Abstractions;
using PadronDeudores.Importador.Worker.Procesamiento;
using PadronDeudores.Infraestructura.Archivos;
using PadronDeudores.Infraestructura.Persistencia;
using PadronDeudores.IntegracionTests.BaseDeDatos;

namespace PadronDeudores.IntegracionTests.Procesamiento;

[Collection(ColeccionBaseDeDatos.Nombre)]
public class ProcesadorImportacionTests(BaseDeDatosFixture fixture)
{
    private const string Muestra = "Muestras/deudores-muestra.txt";

    [Fact]
    public async Task ProcesarAsync_ArchivoReal_CompletaLaImportacionConSuResumen()
    {
        var id = await RegistrarImportacionAsync();

        var resumen = await CrearProcesador().ProcesarAsync(id, Muestra, CancellationToken.None);

        Assert.NotNull(resumen);
        Assert.Equal((5L, 1L, 4L, 2L), (resumen.LineasLeidas, resumen.LineasInvalidas, resumen.Deudores, resumen.Entidades));
        Assert.Equal("completada", await ConsultarAsync<string>("SELECT estado FROM importaciones WHERE id = @id", id));
        Assert.Equal(4L, await ConsultarAsync<long>("SELECT count(*) FROM deudores", id));
    }

    [Fact]
    public async Task ProcesarAsync_MensajeDuplicado_NoVuelveAProcesar()
    {
        var id = await RegistrarImportacionAsync();
        var procesador = CrearProcesador();

        await procesador.ProcesarAsync(id, Muestra, CancellationToken.None);
        var segundaVez = await procesador.ProcesarAsync(id, Muestra, CancellationToken.None);

        Assert.Null(segundaVez);
    }

    private ProcesadorImportacion CrearProcesador() => new(
        fixture.BaseDeDatos,
        new OrigenDisco(),
        new LectorDeudores(NullLogger<LectorDeudores>.Instance),
        new CargadorStaging(fixture.BaseDeDatos),
        new ConsolidadorResumenes(),
        new RepositorioImportaciones(fixture.BaseDeDatos),
        NullLogger<ProcesadorImportacion>.Instance);

    // Lo que hará la API de importación: registrar el pedido como pendiente.
    private async Task<Guid> RegistrarImportacionAsync()
    {
        var id = Guid.CreateVersion7();
        await using var conexion = await fixture.BaseDeDatos.OpenConnectionAsync();
        await conexion.ExecuteAsync(
            "INSERT INTO importaciones (id, origen, archivo, estado) VALUES (@id, 'ruta', @Muestra, 'pendiente')", new { id, Muestra });
        return id;
    }

    private async Task<T> ConsultarAsync<T>(string sql, Guid id)
    {
        await using var conexion = await fixture.BaseDeDatos.OpenConnectionAsync();
        return await conexion.QuerySingleAsync<T>(sql, new { id });
    }
}
