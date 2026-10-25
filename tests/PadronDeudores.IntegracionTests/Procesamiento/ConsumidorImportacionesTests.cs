using Amazon.SQS.Model;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PadronDeudores.Dominio;
using PadronDeudores.Importador.Worker;
using PadronDeudores.Infraestructura.Mensajeria;
using PadronDeudores.Infraestructura.Persistencia;
using PadronDeudores.IntegracionTests.Entorno;

namespace PadronDeudores.IntegracionTests.Procesamiento;

[Collection(ColeccionEntorno.Nombre)]
public class ConsumidorImportacionesTests(EntornoFixture fixture)
{
    // Cola propia del test, para no mezclarse con los mensajes de otros tests.
    private const string Cola = "importaciones-consumidor";

    [Fact]
    public async Task MensajeEnLaCola_CompletaLaImportacionYBorraElMensaje()
    {
        var url = (await fixture.Sqs.CreateQueueAsync(Cola)).QueueUrl;
        var cola = new ColaImportaciones(fixture.Sqs, Options.Create(new OpcionesAws { Region = EntornoFixture.Region, ColaImportaciones = Cola }));
        var archivo = Path.GetFullPath("Muestras/deudores-muestra.txt");
        var id = await fixture.RegistrarImportacionAsync(archivo);
        await cola.EncolarAsync(new MensajeImportacion(id, archivo), CancellationToken.None);

        using var consumidor = new ConsumidorImportaciones(cola, fixture.CrearProcesador(), NullLogger<ConsumidorImportaciones>.Instance);
        await consumidor.StartAsync(CancellationToken.None);
        var colaVacia = await EsperarAsync(async () => await MensajesEnColaAsync(url) == 0);
        await consumidor.StopAsync(CancellationToken.None);

        Assert.True(colaVacia);
        Assert.Equal(EstadoImportacion.Completada,
            await new RepositorioImportaciones(fixture.BaseDeDatos).ObtenerEstadoAsync(id, CancellationToken.None));
    }

    // Visibles + ocultos (los que se están procesando): llega a 0 solo cuando el mensaje se borró.
    private async Task<int> MensajesEnColaAsync(string url)
    {
        var atributos = await fixture.Sqs.GetQueueAttributesAsync(new GetQueueAttributesRequest
        {
            QueueUrl = url,
            AttributeNames = ["ApproximateNumberOfMessages", "ApproximateNumberOfMessagesNotVisible"],
        });
        return atributos.ApproximateNumberOfMessages + atributos.ApproximateNumberOfMessagesNotVisible;
    }

    private static async Task<bool> EsperarAsync(Func<Task<bool>> condicion)
    {
        var limite = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < limite)
        {
            if (await condicion())
            {
                return true;
            }

            await Task.Delay(200);
        }

        return false;
    }
}
