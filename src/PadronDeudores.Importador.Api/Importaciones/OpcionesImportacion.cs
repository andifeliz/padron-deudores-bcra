using System.ComponentModel.DataAnnotations;

namespace PadronDeudores.Importador.Api.Importaciones;

public sealed class OpcionesImportacion
{
    public const string Seccion = "Importacion";

    // Carpeta donde están los archivos que se pueden importar por ruta local.
    [Required]
    public string DirectorioArchivos { get; init; } = "";
}
