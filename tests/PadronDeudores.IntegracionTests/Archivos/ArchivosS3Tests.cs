using System.Security.Cryptography;
using PadronDeudores.Infraestructura.Archivos;
using PadronDeudores.IntegracionTests.Entorno;

namespace PadronDeudores.IntegracionTests.Archivos;

[Collection(ColeccionEntorno.Nombre)]
public class ArchivosS3Tests(EntornoFixture fixture)
{
    [Fact]
    public async Task SubirYAbrir_ArchivoDeVariasPartes_LoDevuelveIgual()
    {
        // 20 MiB: con partes de 8 MiB son 3 partes, la última más chica.
        var contenido = RandomNumberGenerator.GetBytes(20 * 1024 * 1024);
        var archivos = new ArchivosS3(fixture.S3);

        var archivo = await archivos.SubirAsync(EntornoFixture.Bucket, "pruebas/varias-partes.txt", new MemoryStream(contenido), CancellationToken.None);
        await using var leido = await archivos.AbrirAsync(archivo, CancellationToken.None);

        Assert.Equal($"s3://{EntornoFixture.Bucket}/pruebas/varias-partes.txt", archivo);
        Assert.Equal(SHA256.HashData(contenido), await SHA256.HashDataAsync(leido));
    }

    [Fact]
    public async Task SubirAsync_ContenidoVacio_FallaYNoDejaPartesEnS3()
    {
        var archivos = new ArchivosS3(fixture.S3);

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            archivos.SubirAsync(EntornoFixture.Bucket, "pruebas/vacio.txt", new MemoryStream(), CancellationToken.None));

        var pendientes = await fixture.S3.ListMultipartUploadsAsync(EntornoFixture.Bucket);
        Assert.DoesNotContain(pendientes.MultipartUploads ?? [], s => s.Key == "pruebas/vacio.txt");
    }
}
