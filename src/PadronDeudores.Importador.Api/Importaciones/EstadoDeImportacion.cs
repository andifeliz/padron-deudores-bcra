using System.Text.Json.Nodes;

namespace PadronDeudores.Importador.Api.Importaciones;

// Respuesta de GET /importaciones/{id}. El resumen aparece cuando la importación termina.
public sealed record EstadoDeImportacion(
    Guid Id,
    string Origen,
    string Archivo,
    string Estado,
    DateTime CreadaEn,
    DateTime? IniciadaEn,
    DateTime? FinalizadaEn,
    JsonNode? Resumen,
    string? Error);
