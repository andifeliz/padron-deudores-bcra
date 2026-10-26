using Microsoft.Extensions.Options;

namespace PadronDeudores.Infraestructura.Archivos;

// Elige de dónde abrir el archivo: s3://… desde S3; cualquier otra ruta, desde el disco.
// Solo abre archivos de la carpeta de datos o del bucket del proyecto, aunque el mensaje de la cola diga otra cosa.
public sealed class OrigenSegunArchivo(OrigenDisco disco, ArchivosS3 s3, IOptions<OpcionesArchivos> opciones) : IOrigenArchivo
{
    public Task<Stream> AbrirAsync(string archivo, CancellationToken ct) =>
        ArchivosS3.EsDeS3(archivo)
            ? EstaEnElBucket(archivo) ? s3.AbrirAsync(archivo, ct) : throw FueraDeLoPermitido(archivo)
            : EstaEnLaCarpeta(archivo) ? disco.AbrirAsync(archivo, ct) : throw FueraDeLoPermitido(archivo);

    // GetFullPath resuelve los "..": /data/../etc/passwd queda como /etc/passwd y se rechaza.
    private bool EstaEnLaCarpeta(string archivo)
    {
        var carpeta = Path.GetFullPath(opciones.Value.DirectorioArchivos).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return Path.GetFullPath(archivo).StartsWith(carpeta, StringComparison.Ordinal);
    }

    private bool EstaEnElBucket(string archivo) =>
        archivo.StartsWith($"{ArchivosS3.Prefijo}{opciones.Value.BucketArchivos}/", StringComparison.Ordinal);

    private static UnauthorizedAccessException FueraDeLoPermitido(string archivo) =>
        new($"El archivo {archivo} está fuera de la carpeta de datos y del bucket permitidos.");
}
