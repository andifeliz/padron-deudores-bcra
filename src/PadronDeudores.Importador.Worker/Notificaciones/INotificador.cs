namespace PadronDeudores.Importador.Worker.Notificaciones;

// Avisa el resultado de una importación. Si el aviso falla, no falla la importación: ya está guardada.
public interface INotificador
{
    Task NotificarAsync(NotificacionImportacion notificacion, CancellationToken ct);
}
