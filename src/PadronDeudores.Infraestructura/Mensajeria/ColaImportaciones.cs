using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;

namespace PadronDeudores.Infraestructura.Mensajeria;

// Envía y recibe los pedidos de importación de la cola SQS.
public sealed class ColaImportaciones(IAmazonSQS sqs, IOptions<OpcionesAws> opciones)
{
    // Máximo que permite SQS para esperar mensajes en un solo pedido (long polling).
    private const int EsperaMaximaSegundos = 20;

    private static readonly JsonSerializerOptions FormatoJson = new(JsonSerializerDefaults.Web);
    private string? _url;

    public async Task EncolarAsync(MensajeImportacion mensaje, CancellationToken ct) =>
        await sqs.SendMessageAsync(await ObtenerUrlAsync(ct), JsonSerializer.Serialize(mensaje, FormatoJson), ct);

    // Espera hasta 20 s a que llegue un mensaje; devuelve null si no llegó ninguno.
    public async Task<MensajeRecibido?> RecibirAsync(CancellationToken ct)
    {
        var respuesta = await sqs.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = await ObtenerUrlAsync(ct),
            MaxNumberOfMessages = 1,
            WaitTimeSeconds = EsperaMaximaSegundos,
            MessageSystemAttributeNames = ["ApproximateReceiveCount"],
        }, ct);

        return respuesta.Messages is [var mensaje, ..]
            ? new MensajeRecibido(Leer(mensaje.Body), mensaje.ReceiptHandle, Intento(mensaje))
            : null;
    }

    public async Task BorrarAsync(string recibo, CancellationToken ct) =>
        await sqs.DeleteMessageAsync(await ObtenerUrlAsync(ct), recibo, ct);

    // Mantiene el mensaje oculto para los demás workers durante el plazo indicado, contado desde ahora.
    public async Task ExtenderVisibilidadAsync(string recibo, TimeSpan plazo, CancellationToken ct) =>
        await sqs.ChangeMessageVisibilityAsync(await ObtenerUrlAsync(ct), recibo, (int)plazo.TotalSeconds, ct);

    // SQS pide la URL de la cola, no su nombre: se la pide a AWS la primera vez y la guarda para los envíos siguientes.
    private async Task<string> ObtenerUrlAsync(CancellationToken ct) =>
        _url ??= (await sqs.GetQueueUrlAsync(opciones.Value.ColaImportaciones, ct)).QueueUrl;

    private static MensajeImportacion? Leer(string cuerpo)
    {
        try
        {
            return JsonSerializer.Deserialize<MensajeImportacion>(cuerpo, FormatoJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static int Intento(Message mensaje) =>
        mensaje.Attributes?.TryGetValue("ApproximateReceiveCount", out var valor) == true ? int.Parse(valor) : 1;
}
