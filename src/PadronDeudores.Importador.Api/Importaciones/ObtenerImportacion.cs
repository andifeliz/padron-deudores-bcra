using System.Text.Json.Nodes;
using PadronDeudores.Infraestructura.Persistencia;

namespace PadronDeudores.Importador.Api.Importaciones;

// GET /importaciones/{id}: estado de una importación y, al terminar, su resumen.
public static class ObtenerImportacion
{
    public static void MapObtenerImportacion(this IEndpointRouteBuilder app) =>
        app.MapGet("/importaciones/{id:guid}", ManejarAsync);

    private static async Task<IResult> ManejarAsync(Guid id, RepositorioImportaciones importaciones, CancellationToken ct) =>
        await importaciones.ObtenerAsync(id, ct) is { } importacion
            ? Results.Ok(new
            {
                importacion.Id,
                importacion.Origen,
                importacion.Archivo,
                importacion.Estado,
                importacion.CreadaEn,
                importacion.IniciadaEn,
                importacion.FinalizadaEn,
                Resumen = importacion.Resumen is null ? null : JsonNode.Parse(importacion.Resumen),
                importacion.Error,
            })
            : Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Importación no encontrada");
}
