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
    [InlineData(2, "20000000004,00007CN0001")]
    [InlineData(10, "20000000004,00007CN0001,20000000001,20000000002")]
    public async Task GetTop_DevuelveLosMayoresDeudoresOrdenados(int n, string esperados)
    {
        var top = await _api.CreateClient().GetFromJsonAsync<JsonElement[]>($"/deudores/top/{n}");

        Assert.Equal(esperados.Split(','), top!.Select(d => d.GetProperty("nroIdentificacion").GetString()));
    }

    [Fact]
    public async Task GetTop_EmpateEnLaSuma_OrdenaPorIdentificacion()
    {
        await using (var empate = fixture.BaseDeDatos.CreateCommand(
            "INSERT INTO deudores VALUES ('20000000020', 1, 500), ('20000000010', 1, 500)"))
        {
            await empate.ExecuteNonQueryAsync();
        }

        var top = await _api.CreateClient().GetFromJsonAsync<JsonElement[]>("/deudores/top/2");

        Assert.Equal(["20000000010", "20000000020"], top!.Select(d => d.GetProperty("nroIdentificacion").GetString()));
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
    [InlineData("/deudores/top/0")]
    [InlineData("/deudores/top/1001")]
    [InlineData("/deudores/top/abc")]
    public async Task Get_ParametroMalFormado_Devuelve400(string ruta)
    {
        var respuesta = await _api.CreateClient().GetAsync(ruta);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Health_ConLaBaseDisponible_LiveYReadyResponden200()
    {
        var cliente = _api.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync("/health/live")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync("/health/ready")).StatusCode);
    }

    [Fact]
    public async Task Health_SinBase_ReadyDevuelve503YLiveSigue200()
    {
        // Un puerto donde no hay ninguna base escuchando.
        await using var sinBase = new WebApplicationFactory<ConsultaApi::Program>().WithWebHostBuilder(b => b
            .UseSetting("ConnectionStrings:Consulta", "Host=localhost;Port=1;Database=padron;Username=consulta;Password=consulta"));
        var cliente = sinBase.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync("/health/live")).StatusCode);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await cliente.GetAsync("/health/ready")).StatusCode);
    }

    [Fact]
    public async Task OpenApi_DocumentaLosEndpointsYMuestraLaPantallaDePrueba()
    {
        var cliente = _api.CreateClient();

        var documento = await cliente.GetFromJsonAsync<JsonElement>("/openapi/v1.json");
        var rutas = documento.GetProperty("paths").EnumerateObject().Select(ruta => ruta.Name);

        Assert.Equal(["/deudores/{cuit}", "/deudores/top/{n}", "/entidades/{codigo}"], rutas.Order());
        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync("/scalar")).StatusCode);
    }
}
