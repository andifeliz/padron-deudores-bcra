using System.ComponentModel.DataAnnotations;

namespace PadronDeudores.Importador.Worker.Notificaciones;

public sealed class OpcionesWebhook
{
    public const string Seccion = "Webhook";

    // Dirección que recibe el POST al terminar cada importación. Si está vacía, solo se avisa por log.
    [Url]
    public string? Url { get; init; }
}
