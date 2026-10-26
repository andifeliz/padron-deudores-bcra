using System.ComponentModel.DataAnnotations;

namespace PadronDeudores.Infraestructura.Mensajeria;

// Datos para conectarse a AWS, leídos de la sección "Aws" de la configuración.
public sealed class OpcionesAws
{
    public const string Seccion = "Aws";

    // Dirección de LocalStack (ej.: http://localstack:4566). En AWS real va vacía: la dirección sale de la región.
    public string? ServiceUrl { get; init; }

    [Required]
    public string Region { get; init; } = "";

    [Required]
    public string ColaImportaciones { get; init; } = "";

    // Veces que SQS entrega un mensaje antes de pasarlo a la cola de fallidos (maxReceiveCount).
    // En Docker sale de SQS_MAX_INTENTOS, la misma variable con la que se crea la cola.
    [Range(1, 1000)]
    public int IntentosMaximos { get; init; } = 3;
}
