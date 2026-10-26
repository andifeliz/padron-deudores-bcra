namespace PadronDeudores.Importador.Api.Importaciones;

// Solo un nombre de archivo, sin carpetas: evita leer o escribir fuera del lugar previsto (path traversal).
public static class NombreArchivo
{
    // "." y ".." pasan el chequeo de Path.GetFileName, pero son la carpeta actual y la de arriba.
    public static bool EsValido(string? nombre) =>
        !string.IsNullOrEmpty(nombre) && nombre is not ("." or "..") && Path.GetFileName(nombre) == nombre;

    public static IResult Rechazar(string campo) => Results.ValidationProblem(new Dictionary<string, string[]>
    {
        [campo] = ["Indicá solo el nombre del archivo, por ejemplo \"deudores.txt\"."],
    });
}
