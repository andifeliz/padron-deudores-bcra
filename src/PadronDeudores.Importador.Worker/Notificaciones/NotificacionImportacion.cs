using PadronDeudores.Dominio;
using PadronDeudores.Importador.Worker.Procesamiento;

namespace PadronDeudores.Importador.Worker.Notificaciones;

// Aviso del resultado de una importación: el resumen si terminó bien, el error si falló.
public sealed record NotificacionImportacion(
    Guid ImportacionId,
    EstadoImportacion Estado,
    string Archivo,
    ResumenImportacion? Resumen,
    string? Error)
{
    public static NotificacionImportacion Completada(string archivo, ResumenImportacion resumen) =>
        new(resumen.ImportacionId, EstadoImportacion.Completada, archivo, resumen, null);

    public static NotificacionImportacion Fallida(Guid id, string archivo, string error) =>
        new(id, EstadoImportacion.Fallida, archivo, null, error);
}
