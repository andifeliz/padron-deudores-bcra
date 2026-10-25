using System.Globalization;

namespace PadronDeudores.Dominio;

// Lee una línea de ancho fijo de deudores.txt según el diseño de registro del LEAME.
public static class ParserLineaDeudor
{
    public const int LargoLinea = 171;

    // Posiciones de los campos (en el LEAME empiezan en 1; acá, en 0).
    private static readonly Range CodigoEntidad = 0..5;
    private static readonly Range Periodo = 5..11;
    private static readonly Range NroIdentificacion = 13..24;
    private static readonly Range Situacion = 27..29;
    private static readonly Range Prestamos = 29..41;

    // El BCRA escribe los decimales con coma (2406,0) y no con punto: le indicamos a .NET que use la coma.
    private static readonly NumberFormatInfo FormatoImporte = new() { NumberDecimalSeparator = "," };

    public static bool TryParse(string linea, out LineaDeudor resultado)
    {
        resultado = default;
        if (linea.Length != LargoLinea)
        {
            return false;
        }

        var texto = linea.AsSpan();
        if (!short.TryParse(texto[Situacion], NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out var situacion)
            || !decimal.TryParse(texto[Prestamos], NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingWhite, FormatoImporte, out var prestamos))
        {
            return false;
        }

        resultado = new LineaDeudor(linea[CodigoEntidad], linea[Periodo], linea[NroIdentificacion], situacion, prestamos);
        return true;
    }
}
