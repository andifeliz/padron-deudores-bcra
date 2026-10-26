using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using PadronDeudores.IntegracionTests.Entorno;

namespace PadronDeudores.IntegracionTests.Importaciones;

[Collection(ColeccionEntorno.Nombre)]
public class ImportacionesApiTests(EntornoFixture fixture)
{
    [Fact]
    public async Task Post_ArchivoExistente_RegistraLaImportacionYLaEncola()
    {
        await using var api = CrearApi();
        var cliente = api.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/importaciones", new { archivo = "deudores-muestra.txt" });
        var id = (await respuesta.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString();

        Assert.Equal(HttpStatusCode.Accepted, respuesta.StatusCode);
        var consulta = await cliente.GetFromJsonAsync<JsonElement>(respuesta.Headers.Location);
        Assert.Equal("pendiente", consulta.GetProperty("estado").GetString());
        // Hasta 10 mensajes: otros tests también encolan en esta cola.
        var mensajes = await fixture.Sqs.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = (await fixture.Sqs.GetQueueUrlAsync(EntornoFixture.Cola)).QueueUrl,
            MaxNumberOfMessages = 10,
        });
        Assert.Contains(mensajes.Messages, m => m.Body.Contains(id!));
    }

    [Theory]
    [InlineData("../secreto.txt", HttpStatusCode.BadRequest)]
    [InlineData("..", HttpStatusCode.BadRequest)]
    [InlineData("no-existe.txt", HttpStatusCode.NotFound)]
    public async Task Post_ArchivoInvalido_LoRechaza(string archivo, HttpStatusCode esperado)
    {
        await using var api = CrearApi();

        var respuesta = await api.CreateClient().PostAsJsonAsync("/importaciones", new { archivo });

        Assert.Equal(esperado, respuesta.StatusCode);
    }

    [Fact]
    public async Task Get_ImportacionInexistente_Devuelve404()
    {
        await using var api = CrearApi();

        var respuesta = await api.CreateClient().GetAsync($"/importaciones/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task PostUpload_Archivo_LoGuardaEnS3YEncolaLaImportacion()
    {
        await using var api = CrearApi();
        var cliente = api.CreateClient();

        var respuesta = await cliente.PostAsync("/upload?nombre=deudores-muestra.txt", new ByteArrayContent(await File.ReadAllBytesAsync("Muestras/deudores-muestra.txt")));

        Assert.Equal(HttpStatusCode.Accepted, respuesta.StatusCode);
        var importacion = await cliente.GetFromJsonAsync<JsonElement>(respuesta.Headers.Location);
        Assert.Equal("upload", importacion.GetProperty("origen").GetString());
        var archivo = importacion.GetProperty("archivo").GetString()!;
        Assert.StartsWith($"s3://{EntornoFixture.Bucket}/importaciones/", archivo);
        var objeto = await fixture.S3.GetObjectMetadataAsync(EntornoFixture.Bucket, archivo[$"s3://{EntornoFixture.Bucket}/".Length..]);
        Assert.Equal(new FileInfo("Muestras/deudores-muestra.txt").Length, objeto.ContentLength);
    }

    [Theory]
    [InlineData("deudores.txt", 0, HttpStatusCode.BadRequest)]
    [InlineData("../deudores.txt", 10, HttpStatusCode.BadRequest)]
    [InlineData("deudores.txt", 2 * 1024 * 1024, HttpStatusCode.RequestEntityTooLarge)]
    public async Task PostUpload_PedidoInvalido_LoRechaza(string nombre, int bytes, HttpStatusCode esperado)
    {
        // Máximo de 1 MiB, para probar el límite sin mandar gigas.
        await using var api = CrearApi(b => b.UseSetting("Importacion:TamanioMaximoUploadMiB", "1"));

        var respuesta = await api.CreateClient().PostAsync($"/upload?nombre={Uri.EscapeDataString(nombre)}", new ByteArrayContent(new byte[bytes]));

        Assert.Equal(esperado, respuesta.StatusCode);
    }

    [Fact]
    public async Task OpenApi_DocumentaLosEndpoints()
    {
        await using var api = CrearApi();

        var documento = await api.CreateClient().GetFromJsonAsync<JsonElement>("/openapi/v1.json");
        var rutas = documento.GetProperty("paths").EnumerateObject().Select(ruta => ruta.Name);

        Assert.Equal(["/importaciones", "/importaciones/{id}", "/upload"], rutas.Order());
    }

    // La API real, apuntando a la base y a la cola de los contenedores de test.
    private WebApplicationFactory<Program> CrearApi(Action<IWebHostBuilder>? ajustes = null) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseSetting("ConnectionStrings:Importador", fixture.CadenaConexion)
                .UseSetting("Aws:ServiceUrl", fixture.UrlLocalStack)
                .UseSetting("Importacion:DirectorioArchivos", Path.GetFullPath("Muestras"));
            ajustes?.Invoke(b);
        });
}
