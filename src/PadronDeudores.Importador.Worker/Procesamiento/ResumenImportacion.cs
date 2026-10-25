namespace PadronDeudores.Importador.Worker.Procesamiento;

// Resultado de una importación: se guarda en la base, se loguea y se envía en la notificación.
public sealed record ResumenImportacion(
    Guid ImportacionId,
    long LineasLeidas,
    long LineasInvalidas,
    long Deudores,
    long Entidades,
    IReadOnlyDictionary<string, long> LineasPorPeriodo,
    IReadOnlyDictionary<short, long> CodigosSituacionDesconocidos,
    double DuracionSegundos);
