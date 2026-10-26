namespace PadronDeudores.Infraestructura.Archivos;

// Elige de dónde abrir el archivo: s3://… desde S3; cualquier otra ruta, desde el disco.
public sealed class OrigenSegunArchivo(OrigenDisco disco, ArchivosS3 s3) : IOrigenArchivo
{
    public Task<Stream> AbrirAsync(string archivo, CancellationToken ct) =>
        ArchivosS3.EsDeS3(archivo) ? s3.AbrirAsync(archivo, ct) : disco.AbrirAsync(archivo, ct);
}
