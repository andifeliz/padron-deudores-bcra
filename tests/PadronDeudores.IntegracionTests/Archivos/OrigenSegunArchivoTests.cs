using PadronDeudores.IntegracionTests.Entorno;

namespace PadronDeudores.IntegracionTests.Archivos;

[Collection(ColeccionEntorno.Nombre)]
public class OrigenSegunArchivoTests(EntornoFixture fixture)
{
    // Aunque un mensaje de la cola traiga cualquier ruta, el worker solo lee de la carpeta de datos o del bucket del proyecto.
    [Theory]
    [InlineData("/etc/passwd")]
    [InlineData("Muestras/../../secreto.txt")]
    [InlineData("s3://otro-bucket/deudores.txt")]
    public async Task AbrirAsync_FueraDeLoPermitido_LoRechaza(string archivo)
    {
        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => fixture.CrearOrigen().AbrirAsync(archivo, CancellationToken.None));

        Assert.Contains("fuera de la carpeta de datos", error.Message);
    }
}
