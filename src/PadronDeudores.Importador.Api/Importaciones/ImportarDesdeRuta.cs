using Microsoft.Extensions.Options;
using PadronDeudores.Dominio;

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
