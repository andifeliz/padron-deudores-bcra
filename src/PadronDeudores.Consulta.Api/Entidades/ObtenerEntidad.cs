using PadronDeudores.Infraestructura.Persistencia;

namespace PadronDeudores.Consulta.Api.Entidades;

// GET /entidades/{codigo}: suma de préstamos de una entidad.
public static class ObtenerEntidad
{
    private const int LargoCodigo = 5;

    public static void MapObtenerEntidad(this IEndpointRouteBuilder app) =>
        app.MapGet("/entidades/{codigo}", ManejarAsync);

    private static async Task<IResult> ManejarAsync(string codigo, RepositorioEntidades entidades, CancellationToken ct)
    {
        if (codigo.Length is 0 or > LargoCodigo || !codigo.All(char.IsAsciiDigit))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["codigo"] = ["El código de entidad tiene hasta 5 dígitos, por ejemplo 7 o 00007."],
            });
        }

        // El BCRA escribe los códigos con ceros a la izquierda: 7 y 00007 son la misma entidad.
        return await entidades.ObtenerAsync(codigo.PadLeft(LargoCodigo, '0'), ct) is { } entidad
            ? Results.Ok(entidad)
            : Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Entidad no encontrada");
    }
}
