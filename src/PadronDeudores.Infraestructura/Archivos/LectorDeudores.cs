using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using PadronDeudores.Dominio;

namespace PadronDeudores.Infraestructura.Archivos;

// Recorre deudores.txt línea por línea (streaming): entrega las válidas y cuenta y loguea las inválidas.
public sealed class LectorDeudores(ILogger<LectorDeudores> logger)
{
    // Cada cuántas líneas se loguea el avance (el archivo real tiene unos 34 millones).
    private const int LineasPorAvance = 5_000_000;

    public async IAsyncEnumerable<LineaDeudor> LeerAsync(
        Stream archivo, EstadisticasLectura estadisticas, [EnumeratorCancellation] CancellationToken ct)
    {
        // El BCRA publica el archivo en Latin-1 (ISO-8859-1); ReadLineAsync ya quita el CRLF.
        using var lector = new StreamReader(archivo, Encoding.Latin1);
        long numeroLinea = 0;

        while (await lector.ReadLineAsync(ct) is { } linea)
        {
            numeroLinea++;
            if (numeroLinea % LineasPorAvance == 0)
            {
                logger.LogInformation("{Lineas} líneas leídas", numeroLinea);
            }

            if (ParserLineaDeudor.TryParse(linea, out var deudor))
            {
                estadisticas.ContarValida(deudor.Periodo);
                yield return deudor;
            }
            else
            {
                estadisticas.ContarInvalida();
                logger.LogWarning("Línea {NumeroLinea} inválida: se descarta", numeroLinea);
            }
        }
    }
}
