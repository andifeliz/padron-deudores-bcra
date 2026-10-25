namespace PadronDeudores.Infraestructura.Persistencia;

// Una fila de la tabla importaciones. El resumen viene como JSON en texto.
public sealed record Importacion(
    Guid Id,
    string Origen,
    string Archivo,
    string Estado,
    DateTime CreadaEn,
    DateTime? IniciadaEn,
    DateTime? FinalizadaEn,
    string? Resumen,
    string? Error);
