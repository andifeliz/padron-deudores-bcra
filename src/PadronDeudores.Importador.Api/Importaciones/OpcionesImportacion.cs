using System.ComponentModel.DataAnnotations;

namespace PadronDeudores.Importador.Api.Importaciones;

public sealed class OpcionesImportacion
{
    public const string Seccion = "Importacion";

    // Carpeta donde están los archivos que se pueden importar por ruta local.
    [Required]
    public string DirectorioArchivos { get; init; } = "";

    // Bucket de S3 donde se guardan los archivos recibidos por /upload.
    [Required]
    public string BucketArchivos { get; init; } = "";

    // Tamaño máximo de un archivo recibido por /upload (el real pesa unos 5,5 GiB).
    [Range(1, 1024 * 1024)]
    public int TamanioMaximoUploadMiB { get; init; } = 8192;
}
