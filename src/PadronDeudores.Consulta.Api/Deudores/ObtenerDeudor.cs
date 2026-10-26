using PadronDeudores.Infraestructura.Persistencia;

namespace PadronDeudores.Consulta.Api.Deudores;

// GET /deudores/{cuit}: situación máxima y suma de préstamos de un deudor.
public static class ObtenerDeudor
{
    // Las identificaciones del exterior traen letras (ej.: 00007CN0001), por eso no son solo dígitos.
    private const int LargoIdentificacion = 11;

    public static void MapObtenerDeudor(this IEndpointRouteBuilder app) =>
        app.MapGet("/deudores/{cuit}", ManejarAsync);

    private static async Task<IResult> ManejarAsync(string cuit, RepositorioDeudores deudores, CancellationToken ct)
    {
        if (cuit.Length != LargoIdentificacion || !cuit.All(char.IsAsciiLetterOrDigit))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["cuit"] = ["La identificación tiene 11 letras o números, por ejemplo 20123456789."],
            });
        }

        return await deudores.ObtenerAsync(cuit.ToUpperInvariant(), ct) is { } deudor
            ? Results.Ok(deudor)
            : Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Deudor no encontrado");
    }
}
