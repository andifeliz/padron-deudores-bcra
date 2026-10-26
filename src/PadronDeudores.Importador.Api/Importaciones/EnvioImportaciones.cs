using PadronDeudores.Infraestructura.Mensajeria;
using PadronDeudores.Infraestructura.Persistencia;

namespace PadronDeudores.Importador.Api.Importaciones;

// Manda a la cola el mensaje de una importación ya guardada y anota que se envió.
// Primero envía y después anota: si se corta en el medio, el mensaje se reenvía (el worker ignora duplicados), pero nunca se pierde.
public sealed class EnvioImportaciones(ColaImportaciones cola, RepositorioImportaciones importaciones)
{
    public async Task EnviarAsync(Guid id, string archivo, CancellationToken ct)
    {
        await cola.EncolarAsync(new MensajeImportacion(id, archivo), ct);
        await importaciones.MarcarEncoladaAsync(id, ct);
    }
}
