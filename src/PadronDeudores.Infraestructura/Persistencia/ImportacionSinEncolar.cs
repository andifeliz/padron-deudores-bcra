namespace PadronDeudores.Infraestructura.Persistencia;

// Una importación guardada cuyo mensaje todavía no llegó a la cola.
public sealed record ImportacionSinEncolar(Guid Id, string Archivo);
