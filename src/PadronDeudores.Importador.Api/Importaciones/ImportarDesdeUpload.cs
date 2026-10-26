using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using PadronDeudores.Dominio;
using PadronDeudores.Infraestructura.Archivos;

namespace PadronDeudores.Importador.Api.Importaciones;

// POST /upload?nombre=deudores.txt: recibe el archivo en el cuerpo del pedido, lo guarda en S3 y encola su importación.
// El cuerpo se lee en streaming (sin IFormFile): se va subiendo a S3 a medida que llega, sin guardarlo entero en memoria ni en disco.
public static class ImportarDesdeUpload
{
    private const string NombrePorDefecto = "deudores.txt";

    public static void MapImportarDesdeUpload(this IEndpointRouteBuilder app) =>
        app.MapPost("/upload", ManejarAsync)
            .WithTags("Importaciones")
            .WithSummary("Subir un archivo e importarlo")
            .WithDescription("Recibe el archivo en el cuerpo del pedido (application/octet-stream), lo guarda en S3 y encola su importación. El nombre es opcional (por defecto deudores.txt).")
            .Accepts<byte[]>("application/octet-stream")
            .Produces<ImportacionAceptada>(StatusCodes.Status202Accepted)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge);

    private static async Task<IResult> ManejarAsync(
        HttpContext http,
        string? nombre,
        IOptions<OpcionesImportacion> opciones,
        IOptions<OpcionesArchivos> lugares,
        ArchivosS3 archivos,
        RegistroImportacion registro,
        CancellationToken ct)
    {
        nombre ??= NombrePorDefecto;
        if (!NombreArchivo.EsValido(nombre))
        {
            return NombreArchivo.Rechazar("nombre");
        }

        var maximo = opciones.Value.TamanioMaximoUploadMiB * 1024L * 1024;
        if (http.Request.ContentLength > maximo)
        {
            return Demasiado(opciones.Value.TamanioMaximoUploadMiB);
        }

        // Kestrel corta los pedidos de más de 30 MB: el límite se sube solo para este endpoint.
        if (http.Features.Get<IHttpMaxRequestBodySizeFeature>() is { IsReadOnly: false } limite)
        {
            limite.MaxRequestBodySize = maximo;
        }

        var id = Guid.CreateVersion7();
        string archivo;
        try
        {
            archivo = await archivos.SubirAsync(lugares.Value.BucketArchivos, $"importaciones/{id}/{nombre}", http.Request.Body, ct);
        }
        catch (InvalidDataException ex)
        {
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Archivo vacío", detail: ex.Message);
        }
        catch (BadHttpRequestException ex) when (ex.StatusCode == StatusCodes.Status413PayloadTooLarge)
        {
            // Pasa cuando el cliente no informa el tamaño (envío por partes) y se supera el máximo mientras llega.
            return Demasiado(opciones.Value.TamanioMaximoUploadMiB);
        }

        return await registro.RegistrarYEncolarAsync(id, OrigenImportacion.Upload, archivo, ct);
    }

    private static IResult Demasiado(int maximoMiB) => Results.Problem(
        statusCode: StatusCodes.Status413PayloadTooLarge, title: "Archivo demasiado grande", detail: $"El máximo es {maximoMiB} MiB.");
}
