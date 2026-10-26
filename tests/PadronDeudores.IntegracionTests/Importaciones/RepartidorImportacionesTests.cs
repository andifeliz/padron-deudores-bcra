using Amazon.SQS.Model;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PadronDeudores.Importador.Api.Importaciones;
using PadronDeudores.Infraestructura.Mensajeria;
using PadronDeudores.Infraestructura.Persistencia;
using PadronDeudores.IntegracionTests.Entorno;

namespace PadronDeudores.IntegracionTests.Importaciones;

[Collection(ColeccionEntorno.Nombre)]
public class RepartidorImportacionesTests(EntornoFixture fixture)
{
    // Cola propia del test, para no mezclarse con los mensajes de otros tests.
    private const string Cola = "importaciones-repartidor";

    [Fact]
    public async Task RepartirAsync_EnviaSoloLasPendientesSinEncolarConAntiguedad()
    {
        var url = (await fixture.Sqs.CreateQueueAsync(Cola)).QueueUrl;
        var trabada = await RegistrarAsync(creadaHace: "1 minute", encolada: false);
        var recien = await RegistrarAsync(creadaHace: "0 seconds", encolada: false);
        var yaEncolada = await RegistrarAsync(creadaHace: "1 minute", encolada: true);

        await CrearRepartidor().RepartirAsync(CancellationToken.None);

        var mensajes = (await fixture.Sqs.ReceiveMessageAsync(new ReceiveMessageRequest { QueueUrl = url, MaxNumberOfMessages = 10 })).Messages ?? [];
        Assert.Contains(mensajes, m => m.Body.Contains(trabada.ToString()));
        Assert.DoesNotContain(mensajes, m => m.Body.Contains(recien.ToString()) || m.Body.Contains(yaEncolada.ToString()));
        Assert.Equal((true, false), (await EstaEncoladaAsync(trabada), await EstaEncoladaAsync(recien)));
    }

    private RepartidorImportaciones CrearRepartidor()
    {
        var importaciones = new RepositorioImportaciones(fixture.BaseDeDatos);
        var cola = new ColaImportaciones(fixture.Sqs, Options.Create(new OpcionesAws { Region = EntornoFixture.Region, ColaImportaciones = Cola }));
        return new RepartidorImportaciones(importaciones, new EnvioImportaciones(cola, importaciones), NullLogger<RepartidorImportaciones>.Instance);
    }

    private async Task<Guid> RegistrarAsync(string creadaHace, bool encolada)
    {
        var id = Guid.CreateVersion7();
        await using var alta = fixture.BaseDeDatos.CreateCommand($"""
            INSERT INTO importaciones (id, origen, archivo, estado, creada_en, encolada_en)
            VALUES ($1, 'ruta', 'Muestras/deudores-muestra.txt', 'pendiente', now() - interval '{creadaHace}', {(encolada ? "now()" : "null")})
            """);
        alta.Parameters.AddWithValue(id);
        await alta.ExecuteNonQueryAsync();
        return id;
    }

    private async Task<bool> EstaEncoladaAsync(Guid id)
    {
        await using var consulta = fixture.BaseDeDatos.CreateCommand("SELECT encolada_en IS NOT NULL FROM importaciones WHERE id = $1");
        consulta.Parameters.AddWithValue(id);
        return (bool)(await consulta.ExecuteScalarAsync())!;
    }
}
