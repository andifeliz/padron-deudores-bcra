using Microsoft.Extensions.Options;
using PadronDeudores.Dominio;
using PadronDeudores.Infraestructura.Mensajeria;
using PadronDeudores.Infraestructura.Persistencia;

namespace PadronDeudores.Importador.Api.Importaciones;

// POST /importaciones: registra una importación de un archivo de la carpeta de datos y la encola.
public static class ImportarDesdeRuta
{
    public sealed record Pedido(string Archivo);

    public static void MapImportarDesdeRuta(this IEndpointRouteBuilder app) =>
        app.MapPost("/importaciones", ManejarAsync);

    private static async Task<IResult> ManejarAsync(
        Pedido pedido,
        IOptions<OpcionesImportacion> opciones,
        RepositorioImportaciones importaciones,
        ColaImportaciones cola,
        ILoggerFactory logs,
        CancellationToken ct)
    {
        var logger = logs.CreateLogger(nameof(ImportarDesdeRuta));

        // Solo un nombre de archivo, sin carpetas: evita leer archivos fuera de la carpeta de datos.
        var nombre = Path.GetFileName(pedido.Archivo ?? "");
        if (nombre.Length == 0 || nombre != pedido.Archivo)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["archivo"] = ["Indicá solo el nombre del archivo, por ejemplo \"deudores.txt\"."],
            });
        }

        var ruta = Path.Combine(opciones.Value.DirectorioArchivos, nombre);
        if (!File.Exists(ruta))
        {
            return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Archivo no encontrado",
                detail: $"No existe {nombre} en la carpeta de datos.");
        }

        var id = Guid.CreateVersion7();
        await importaciones.RegistrarAsync(id, OrigenImportacion.Ruta, ruta, ct);

        try
        {
            await cola.EncolarAsync(new MensajeImportacion(id, ruta), ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "No se pudo encolar la importación {ImportacionId}", id);
            await importaciones.MarcarFallidaAsync(id, "No se pudo encolar el pedido.", CancellationToken.None);
            return Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Cola no disponible");
        }

        logger.LogInformation("Importación {ImportacionId} encolada: {Archivo}", id, ruta);
        return Results.Accepted($"/importaciones/{id}", new { id, estado = "pendiente" });
    }
}
