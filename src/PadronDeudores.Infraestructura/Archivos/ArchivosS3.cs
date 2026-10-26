using Amazon.S3;
using Amazon.S3.Model;

namespace PadronDeudores.Infraestructura.Archivos;

// Guarda y lee archivos en S3. Un archivo de S3 se identifica como s3://bucket/clave.
public sealed class ArchivosS3(IAmazonS3 s3) : IOrigenArchivo
{
    public const string Prefijo = "s3://";

    // S3 pide partes de al menos 5 MiB (salvo la última). Es la memoria que usa la subida, sea cual sea el tamaño del archivo.
    private const int TamanioParte = 8 * 1024 * 1024;

    public static bool EsDeS3(string archivo) => archivo.StartsWith(Prefijo, StringComparison.Ordinal);

    // Sube el contenido por partes a medida que llega (multipart) y devuelve su s3://bucket/clave.
    public async Task<string> SubirAsync(string bucket, string clave, Stream contenido, CancellationToken ct)
    {
        var subida = await s3.InitiateMultipartUploadAsync(bucket, clave, ct);

        try
        {
            var partes = new List<UploadPartResponse>();
            var buffer = new byte[TamanioParte];
            int leidos;
            while ((leidos = await contenido.ReadAtLeastAsync(buffer, buffer.Length, throwOnEndOfStream: false, ct)) > 0)
            {
                using var parte = new MemoryStream(buffer, 0, leidos);
                partes.Add(await s3.UploadPartAsync(new UploadPartRequest
                {
                    BucketName = bucket,
                    Key = clave,
                    UploadId = subida.UploadId,
                    PartNumber = partes.Count + 1,
                    InputStream = parte,
                }, ct));
            }

            if (partes.Count == 0)
            {
                throw new InvalidDataException("El archivo está vacío.");
            }

            var fin = new CompleteMultipartUploadRequest { BucketName = bucket, Key = clave, UploadId = subida.UploadId };
            fin.AddPartETags(partes);
            await s3.CompleteMultipartUploadAsync(fin, ct);
            return $"{Prefijo}{bucket}/{clave}";
        }
        catch
        {
            // Sin esto, S3 conserva (y cobra) las partes ya subidas de una subida que nunca se completa.
            await s3.AbortMultipartUploadAsync(bucket, clave, subida.UploadId, CancellationToken.None);
            throw;
        }
    }

    // Devuelve el contenido en streaming: se lee a medida que llega de S3, sin descargarlo antes.
    public async Task<Stream> AbrirAsync(string archivo, CancellationToken ct)
    {
        var (bucket, clave) = Separar(archivo);
        var respuesta = await s3.GetObjectAsync(bucket, clave, ct);
        return respuesta.ResponseStream;
    }

    private static (string Bucket, string Clave) Separar(string archivo)
    {
        var ruta = archivo[Prefijo.Length..];
        var barra = ruta.IndexOf('/');
        return barra > 0 ? (ruta[..barra], ruta[(barra + 1)..]) : throw new ArgumentException($"Archivo de S3 inválido: {archivo}");
    }
}
