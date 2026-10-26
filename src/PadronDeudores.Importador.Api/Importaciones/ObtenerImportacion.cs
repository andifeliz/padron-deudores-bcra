using System.Text.Json.Nodes;
using PadronDeudores.Infraestructura.Persistencia;

namespace PadronDeudores.Importador.Api.Importaciones;

// GET /importaciones/{id}: estado de una importación y, al terminar, su resumen.
public static class ObtenerImportacion
{
    public static void MapObtenerImportacion(this IEndpointRouteBuilder app) =>
        app.MapGet("/importaciones/{id:guid}", ManejarAsync)
            .WithTags("Importaciones")
            .WithSummary("Estado de una importación")
            .WithDescription("Devuelve el estado (pendiente, procesando, completada o fallida) y, al terminar, el resumen de la carga.")
            .Produces<EstadoDeImportacion>()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> ManejarAsync(Guid id, RepositorioImportaciones importaciones, CancellationToken ct) =>
        await importaciones.ObtenerAsync(id, ct) is { } fila
            ? Results.Ok(new EstadoDeImportacion(fila.Id, fila.Origen, fila.Archivo, fila.Estado, fila.CreadaEn, fila.IniciadaEn,
                fila.FinalizadaEn, fila.Resumen is null ? null : JsonNode.Parse(fila.Resumen), fila.Error))
            : Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Importación no encontrada");
}
