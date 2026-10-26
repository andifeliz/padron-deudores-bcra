using System.ComponentModel.DataAnnotations;

namespace PadronDeudores.Consulta.Api.Deudores;

public sealed class OpcionesConsulta
{
    public const string Seccion = "Consulta";

    // Máximo de deudores que se pueden pedir en el top: evita respuestas enormes.
    [Range(1, 100_000)]
    public int TopMaximo { get; init; } = 1000;
}
