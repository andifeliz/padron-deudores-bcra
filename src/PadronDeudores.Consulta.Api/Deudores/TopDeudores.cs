using Microsoft.Extensions.Options;
using PadronDeudores.Dominio;
using PadronDeudores.Infraestructura.Persistencia;

namespace PadronDeudores.Consulta.Api.Deudores;

// GET /deudores/top/{n}?situacion=5: los N deudores con mayor suma de préstamos, opcionalmente de una situación.
public static class TopDeudores
{
    private static readonly string CodigosValidos = string.Join(", ", Enum.GetValues<Situacion>().Select(s => (short)s));

    public static void MapTopDeudores(this IEndpointRouteBuilder app) =>
        app.MapGet("/deudores/top/{n}", ManejarAsync)
            .WithTags("Deudores")
            .WithSummary("Los N deudores con mayor suma de préstamos")
            .WithDescription("Ordenados por suma total de préstamos (en miles de pesos) de mayor a menor; si hay empate, por identificación de menor a mayor. "
                + "N va de 1 al máximo configurado (1000 por defecto). Filtro opcional por una situación máxima, por código exacto: "
                + "?situacion=5 (situacion=1 no incluye 11; situacion=2 no incluye 21, 22 ni 23).")
            .Produces<IReadOnlyList<Deudor>>()
            .ProducesValidationProblem();

    private static async Task<IResult> ManejarAsync(
        int n,
        short? situacion,
        IOptions<OpcionesConsulta> opciones,
        RepositorioDeudores deudores,
        CancellationToken ct)
    {
        var maximo = opciones.Value.TopMaximo;
        if (n < 1 || n > maximo)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["n"] = [$"N tiene que estar entre 1 y {maximo}."],
            });
        }

        if (situacion is { } codigo && !Enum.IsDefined((Situacion)codigo))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["situacion"] = [$"Situación inválida. Los códigos válidos son {CodigosValidos}."],
            });
        }

        return Results.Ok(await deudores.ObtenerTopAsync(n, situacion, ct));
    }
}
