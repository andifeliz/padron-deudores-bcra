using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace PadronDeudores.Importador.Worker.Notificaciones;

// Envía el resultado de la importación en JSON, con un POST a la URL configurada.
public sealed class NotificadorWebhook(
    IHttpClientFactory clientes,
    IOptions<OpcionesWebhook> opciones,
    ILogger<NotificadorWebhook> logger) : INotificador
{
    // Nombre del cliente HTTP: en Program.cs se le configuran los reintentos y el timeout.
    public const string Cliente = "webhook";

    private static readonly JsonSerializerOptions FormatoJson = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public async Task NotificarAsync(NotificacionImportacion notificacion, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(opciones.Value.Url))
        {
            logger.LogInformation("Webhook no configurado: el resultado queda solo en el log");
            return;
        }

        try
        {
            // El JSON ya armado (con Content-Length) se puede reenviar igual en cada reintento.
            using var cuerpo = new StringContent(JsonSerializer.Serialize(notificacion, FormatoJson), Encoding.UTF8, "application/json");
            using var respuesta = await clientes.CreateClient(Cliente).PostAsync(opciones.Value.Url, cuerpo, ct);
            respuesta.EnsureSuccessStatusCode();
            logger.LogInformation("Webhook notificado: importación {ImportacionId} {Estado}", notificacion.ImportacionId, notificacion.Estado);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            // Se agotaron los reintentos: se registra y se sigue.
            logger.LogError(ex, "No se pudo notificar al webhook la importación {ImportacionId}", notificacion.ImportacionId);
        }
    }
}
