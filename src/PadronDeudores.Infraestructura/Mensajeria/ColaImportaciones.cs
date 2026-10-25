using System.Text.Json;
using Amazon.SQS;
using Microsoft.Extensions.Options;

namespace PadronDeudores.Infraestructura.Mensajeria;

// Envía los pedidos de importación a la cola SQS.
public sealed class ColaImportaciones(IAmazonSQS sqs, IOptions<OpcionesAws> opciones)
{
    private static readonly JsonSerializerOptions FormatoJson = new(JsonSerializerDefaults.Web);
    private string? _url;

    public async Task EncolarAsync(MensajeImportacion mensaje, CancellationToken ct) =>
        await sqs.SendMessageAsync(await ObtenerUrlAsync(ct), JsonSerializer.Serialize(mensaje, FormatoJson), ct);

    // SQS pide la URL de la cola, no su nombre: se la pide a AWS la primera vez y la guarda para los envíos siguientes.
    private async Task<string> ObtenerUrlAsync(CancellationToken ct) =>
        _url ??= (await sqs.GetQueueUrlAsync(opciones.Value.ColaImportaciones, ct)).QueueUrl;
}
