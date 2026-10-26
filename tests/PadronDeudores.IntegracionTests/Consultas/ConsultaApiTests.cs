extern alias ConsultaApi;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PadronDeudores.IntegracionTests.Entorno;

namespace PadronDeudores.IntegracionTests.Consultas;

[Collection(ColeccionEntorno.Nombre)]
public class ConsultaApiTests(EntornoFixture fixture) : IAsyncLifetime
{
    private const string Muestra = "Muestras/deudores-muestra.txt";

    private WebApplicationFactory<ConsultaApi::Program> _api = null!;

    // Importa la muestra con el procesador real: así se prueba que el usuario de consulta puede leer las tablas nuevas.
    public async Task InitializeAsync()
    {
        var id = await fixture.RegistrarImportacionAsync(Muestra);
        await fixture.CrearProcesador().ProcesarAsync(id, Muestra, CancellationToken.None);
        _api = new WebApplicationFactory<ConsultaApi::Program>().WithWebHostBuilder(b => b
            .UseSetting("ConnectionStrings:Consulta", fixture.CadenaConexionConsulta));
    }

    public async Task DisposeAsync() => await _api.DisposeAsync();

    [Theory]
    [InlineData("20000000004", "20000000004", 5, 261.0)]
    [InlineData("00007cn0001", "00007CN0001", 1, 44.0)]
    public async Task GetDeudor_Existente_DevuelveSuResumen(string pedido, string identificacion, short situacion, double suma)
    {
        var deudor = await _api.CreateClient().GetFromJsonAsync<JsonElement>($"/deudores/{pedido}");

        Assert.Equal(identificacion, deudor.GetProperty("nroIdentificacion").GetString());
        Assert.Equal(situacion, deudor.GetProperty("situacionMaxima").GetInt16());
        Assert.Equal((decimal)suma, deudor.GetProperty("sumaTotalPrestamos").GetDecimal());
    }

    [Theory]
    [InlineData("00007")]
    [InlineData("7")]
    public async Task GetEntidad_Existente_DevuelveSuResumenConElCodigoCompleto(string codigo)
    {
        var entidad = await _api.CreateClient().GetFromJsonAsync<JsonElement>($"/entidades/{codigo}");

        Assert.Equal("00007", entidad.GetProperty("codigoEntidad").GetString());
        Assert.Equal(45.0m, entidad.GetProperty("sumaTotalPrestamos").GetDecimal());
    }

    [Theory]
    [InlineData("/deudores/20000000009")]
    [InlineData("/entidades/99999")]
    public async Task Get_Inexistente_Devuelve404(string ruta)
    {
        var respuesta = await _api.CreateClient().GetAsync(ruta);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Theory]
    [InlineData("/deudores/123")]
    [InlineData("/deudores/2000000000-")]
    [InlineData("/entidades/abc")]
    [InlineData("/entidades/123456")]
    public async Task Get_ParametroMalFormado_Devuelve400(string ruta)
    {
        var respuesta = await _api.CreateClient().GetAsync(ruta);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }
}
