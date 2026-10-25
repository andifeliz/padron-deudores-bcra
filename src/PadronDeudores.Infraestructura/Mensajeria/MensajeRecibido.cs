namespace PadronDeudores.Infraestructura.Mensajeria;

// Un mensaje leído de la cola. Contenido es null si el JSON no es válido; Recibo sirve para borrarlo o renovarlo.
public sealed record MensajeRecibido(MensajeImportacion? Contenido, string Recibo, int Intento);
