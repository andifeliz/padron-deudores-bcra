using Microsoft.Extensions.Logging.Abstractions;
using PadronDeudores.Infraestructura.Archivos;

namespace PadronDeudores.IntegracionTests.Archivos;

public class LectorDeudoresTests
{
    // 5 líneas reales con CRLF y en Latin-1, como el archivo del BCRA: 4 válidas (dos períodos) y 1 cortada.
    private const string Muestra = "Muestras/deudores-muestra.txt";

    [Fact]
    public async Task LeerAsync_ArchivoReal_EntregaLasValidasYCuentaTodas()
    {
        var estadisticas = new EstadisticasLectura();
        await using var archivo = File.OpenRead(Muestra);

        var deudores = await new LectorDeudores(NullLogger<LectorDeudores>.Instance)
            .LeerAsync(archivo, estadisticas, CancellationToken.None)
            .ToListAsync();

        Assert.Equal(4, deudores.Count);
        Assert.Equal(5, estadisticas.LineasLeidas);
        Assert.Equal(1, estadisticas.LineasInvalidas);
        Assert.Equal(3, estadisticas.LineasPorPeriodo["202311"]);
        Assert.Equal(1, estadisticas.LineasPorPeriodo["202310"]);
    }
}
