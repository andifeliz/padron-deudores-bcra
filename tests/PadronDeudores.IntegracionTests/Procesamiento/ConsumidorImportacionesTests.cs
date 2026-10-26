using Amazon.SQS.Model;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PadronDeudores.Dominio;
using PadronDeudores.Importador.Worker;
using PadronDeudores.Infraestructura.Mensajeria;
using PadronDeudores.Infraestructura.Persistencia;
using PadronDeudores.IntegracionTests.Entorno;
using PadronDeudores.IntegracionTests.Notificaciones;

namespace PadronDeudores.IntegracionTests.Procesamiento;

[Collection(ColeccionEntorno.Nombre)]
public class ConsumidorImportacionesTests(EntornoFixture fixture)
{
    // Colas propias del test, para no mezclarse con los mensajes de otros tests.
    private const string Cola = "importaciones-consumidor";
    private const string ColaConReintentos = "importaciones-consumidor-reintentos";
    private const string ColaFallidos = "importaciones-consumidor-fallidas";
    private const int IntentosMaximos = 3;

    [Fact]
    public async Task MensajeEnLaCola_CompletaLaImportacionYBorraElMensaje()
    {
        var url = (await fixture.Sqs.CreateQueueAsync(Cola)).QueueUrl;
        var cola = CrearCola(Cola);
        var archivo = Path.GetFullPath("Muestras/deudores-muestra.txt");
        var id = await fixture.RegistrarImportacionAsync(archivo);
        await cola.EncolarAsync(new MensajeImportacion(id, archivo), CancellationToken.None);

        using var consumidor = CrearConsumidor(cola, new NotificadorEnMemoria());
        await consumidor.StartAsync(CancellationToken.None);
        var colaVacia = await EsperarAsync(async () => await MensajesEnColaAsync(url) == 0);
        await consumidor.StopAsync(CancellationToken.None);

        Assert.True(colaVacia);
        Assert.Equal(EstadoImportacion.Completada,
            await new RepositorioImportaciones(fixture.BaseDeDatos).ObtenerEstadoAsync(id, CancellationToken.None));
    }

    [Fact]
    public async Task ImportacionQueFallaSiempre_AvisaUnaSolaVezAlAgotarLosIntentos()
    {
        var urlFallidos = await CrearColaConFallidosAsync();
        var cola = CrearCola(ColaConReintentos);
        var id = await fixture.RegistrarImportacionAsync("Muestras/no-existe.txt");
        await cola.EncolarAsync(new MensajeImportacion(id, "Muestras/no-existe.txt"), CancellationToken.None);
        var notificador = new NotificadorEnMemoria();

        using var consumidor = CrearConsumidor(cola, notificador);
        await consumidor.StartAsync(CancellationToken.None);
        var pasoAFallidos = await EsperarAsync(async () => await MensajesEnColaAsync(urlFallidos) == 1);
        await consumidor.StopAsync(CancellationToken.None);

        Assert.True(pasoAFallidos);
        Assert.Equal(EstadoImportacion.Fallida, Assert.Single(notificador.Enviadas).Estado);
    }

    private ColaImportaciones CrearCola(string nombre) => new(fixture.Sqs, Options.Create(
        new OpcionesAws { Region = EntornoFixture.Region, ColaImportaciones = nombre, IntentosMaximos = IntentosMaximos }));

    private ConsumidorImportaciones CrearConsumidor(ColaImportaciones cola, NotificadorEnMemoria notificador) => new(
        cola,
        fixture.CrearProcesador(),
        notificador,
        Options.Create(new OpcionesAws { Region = EntornoFixture.Region, ColaImportaciones = "", IntentosMaximos = IntentosMaximos }),
        NullLogger<ConsumidorImportaciones>.Instance);

    // Igual que la cola real, pero el mensaje reaparece al segundo (y no a los 5 minutos) para que el test sea rápido.
    private async Task<string> CrearColaConFallidosAsync()
    {
        var urlFallidos = (await fixture.Sqs.CreateQueueAsync(ColaFallidos)).QueueUrl;
        var arnFallidos = (await fixture.Sqs.GetQueueAttributesAsync(urlFallidos, ["QueueArn"])).QueueARN;
        await fixture.Sqs.CreateQueueAsync(new CreateQueueRequest
        {
            QueueName = ColaConReintentos,
            Attributes = new Dictionary<string, string>
            {
                ["VisibilityTimeout"] = "1",
                ["RedrivePolicy"] = $$"""{"deadLetterTargetArn":"{{arnFallidos}}","maxReceiveCount":"{{IntentosMaximos}}"}""",
            },
        });
        return urlFallidos;
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
