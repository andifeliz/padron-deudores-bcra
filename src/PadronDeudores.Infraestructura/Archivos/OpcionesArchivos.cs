using System.ComponentModel.DataAnnotations;

namespace PadronDeudores.Infraestructura.Archivos;

// Lugares permitidos para los archivos a importar. Los usan la API (para recibirlos) y el worker (para leerlos).
public sealed class OpcionesArchivos
{
    public const string Seccion = "Importacion";

    // Carpeta de los archivos que se importan por ruta local.
    [Required]
    public string DirectorioArchivos { get; init; } = "";

    // Bucket de S3 donde se guardan los archivos recibidos por /upload.
    [Required]
    public string BucketArchivos { get; init; } = "";
}
