namespace PadronDeudores.Importador.Api.Importaciones;

// Solo un nombre de archivo, sin carpetas: evita leer o escribir fuera del lugar previsto (path traversal).
public static class NombreArchivo
{
    public static bool EsValido(string? nombre) => !string.IsNullOrEmpty(nombre) && Path.GetFileName(nombre) == nombre;

    public static IResult Rechazar(string campo) => Results.ValidationProblem(new Dictionary<string, string[]>
    {
        [campo] = ["Indicá solo el nombre del archivo, por ejemplo \"deudores.txt\"."],
    });
}
