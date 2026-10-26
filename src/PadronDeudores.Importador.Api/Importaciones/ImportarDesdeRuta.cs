using Microsoft.Extensions.Options;
using PadronDeudores.Dominio;

namespace PadronDeudores.Importador.Api.Importaciones;

// POST /importaciones: registra una importación de un archivo de la carpeta de datos y la encola.
public static class ImportarDesdeRuta
{
    public sealed record Pedido(string Archivo);

    public static void MapImportarDesdeRuta(this IEndpointRouteBuilder app) =>
        app.MapPost("/importaciones", ManejarAsync)
            .WithTags("Importaciones")
            .WithSummary("Importar un archivo de la carpeta de datos")
            .WithDescription("Recibe el nombre de un archivo de la carpeta de datos (por ejemplo deudores.txt), registra la importación y la encola. Responde enseguida: el procesamiento sigue en segundo plano.")
            .Produces<ImportacionAceptada>(StatusCodes.Status202Accepted)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

    private static async Task<IResult> ManejarAsync(
        Pedido pedido,
        IOptions<OpcionesImportacion> opciones,
        RegistroImportacion registro,
        CancellationToken ct)
    {
        if (!NombreArchivo.EsValido(pedido.Archivo))
        {
            return NombreArchivo.Rechazar("archivo");
        }

        var ruta = Path.Combine(opciones.Value.DirectorioArchivos, pedido.Archivo);
        if (!File.Exists(ruta))
        {
            return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Archivo no encontrado",
                detail: $"No existe {pedido.Archivo} en la carpeta de datos.");
        }

        return await registro.RegistrarYEncolarAsync(Guid.CreateVersion7(), OrigenImportacion.Ruta, ruta, ct);
    }
}
