using Microsoft.Extensions.Options;
using PadronDeudores.Infraestructura.Persistencia;

namespace PadronDeudores.Consulta.Api.Deudores;

// GET /deudores/top/{n}: los N deudores con mayor suma de préstamos.
public static class TopDeudores
{
    public static void MapTopDeudores(this IEndpointRouteBuilder app) =>
        app.MapGet("/deudores/top/{n}", ManejarAsync)
            .WithTags("Deudores")
            .WithSummary("Los N deudores con mayor suma de préstamos")
            .WithDescription("Ordenados por suma total de préstamos (en miles de pesos) de mayor a menor; si hay empate, por identificación de menor a mayor. N va de 1 al máximo configurado (1000 por defecto).")
            .Produces<IReadOnlyList<Deudor>>()
            .ProducesValidationProblem();

    private static async Task<IResult> ManejarAsync(int n, IOptions<OpcionesConsulta> opciones, RepositorioDeudores deudores, CancellationToken ct)
    {
        var maximo = opciones.Value.TopMaximo;
        if (n < 1 || n > maximo)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["n"] = [$"N tiene que estar entre 1 y {maximo}."],
            });
        }

        return Results.Ok(await deudores.ObtenerTopAsync(n, ct));
    }
}
