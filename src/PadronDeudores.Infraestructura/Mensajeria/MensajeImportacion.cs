namespace PadronDeudores.Infraestructura.Mensajeria;

// Mensaje de la cola: qué importación procesar y dónde está el archivo. Lo envía la API y lo lee el worker.
public sealed record MensajeImportacion(Guid ImportacionId, string Archivo);
