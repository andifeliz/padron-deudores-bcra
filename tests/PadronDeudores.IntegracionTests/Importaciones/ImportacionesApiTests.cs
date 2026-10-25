using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
        var mensajes = await fixture.Sqs.ReceiveMessageAsync((await fixture.Sqs.GetQueueUrlAsync(EntornoFixture.Cola)).QueueUrl);
        Assert.Contains(mensajes.Messages, m => m.Body.Contains(id!));
    }

    [Theory]
    [InlineData("../secreto.txt", HttpStatusCode.BadRequest)]
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

    // La API real, apuntando a la base y a la cola de los contenedores de test.
    private WebApplicationFactory<Program> CrearApi() => new WebApplicationFactory<Program>().WithWebHostBuilder(b => b
        .UseSetting("ConnectionStrings:Importador", fixture.CadenaConexion)
        .UseSetting("Aws:ServiceUrl", fixture.UrlLocalStack)
        .UseSetting("Importacion:DirectorioArchivos", Path.GetFullPath("Muestras")));
}
