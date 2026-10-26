using PadronDeudores.Importador.Worker.Notificaciones;

namespace PadronDeudores.IntegracionTests.Notificaciones;

// Notificador de prueba: guarda los avisos en una lista para poder revisarlos.
public sealed class NotificadorEnMemoria : INotificador
{
    public List<NotificacionImportacion> Enviadas { get; } = [];

    public Task NotificarAsync(NotificacionImportacion notificacion, CancellationToken ct)
    {
        Enviadas.Add(notificacion);
        return Task.CompletedTask;
    }
}
