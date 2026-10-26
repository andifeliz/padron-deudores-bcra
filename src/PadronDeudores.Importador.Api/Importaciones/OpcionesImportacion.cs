using System.ComponentModel.DataAnnotations;

namespace PadronDeudores.Importador.Api.Importaciones;

public sealed class OpcionesImportacion
{
    public const string Seccion = "Importacion";

    // Tamaño máximo de un archivo recibido por /upload (el real pesa unos 5,5 GiB).
    [Range(1, 1024 * 1024)]
    public int TamanioMaximoUploadMiB { get; init; } = 8192;
}
