using PadronDeudores.Dominio;

namespace PadronDeudores.Dominio.Tests;

// Líneas reales de deudores.txt (noviembre 2023), con la identificación de personas reemplazada por una ficticia.
public class ParserLineaDeudorTests
{
    private const string LineaNormal = "0000720231111200000000010001 1,0         ,0          ,0          ,0          ,0          ,0          1,0         ,0          ,0          ,0          0           0000000   ";
    private const string LineaPrestamoCero = "0000720231111200000000020005 ,0          ,0          ,0          34,0        ,0          ,0          ,0          ,0          ,0          34,0        0           0000000   ";
    private const string LineaSituacionOnce = "00007202311112000000000300011162,0       ,0          ,0          ,0          162,0       ,0          ,0          ,0          ,0          ,0          0           1000000   ";
    private const string LineaExterior = "000072023119800007CN00016411 44,0        ,0          108992,0    ,0          ,0          ,0          44,0        ,0          ,0          108992,0    0           0000000   ";

    [Fact]
    public void TryParse_LineaReal_ExtraeLosCampos()
    {
        Assert.True(ParserLineaDeudor.TryParse(LineaNormal, out var linea));
        Assert.Equal(new LineaDeudor("00007", "202311", "20000000001", 1, 1.0m), linea);
    }

    [Fact]
    public void TryParse_SituacionDeDosDigitos_LaLeeCompleta()
    {
        Assert.True(ParserLineaDeudor.TryParse(LineaSituacionOnce, out var linea));
        Assert.Equal(11, linea.Situacion);
    }

    [Fact]
    public void TryParse_PrestamoSinParteEntera_DevuelveCero()
    {
        Assert.True(ParserLineaDeudor.TryParse(LineaPrestamoCero, out var linea));
        Assert.Equal(0m, linea.Prestamos);
    }

    [Fact]
    public void TryParse_IdentificacionDelExterior_ConservaLasLetras()
    {
        Assert.True(ParserLineaDeudor.TryParse(LineaExterior, out var linea));
        Assert.Equal("00007CN0001", linea.NroIdentificacion);
    }

    [Fact]
    public void TryParse_LargoInvalido_DevuelveFalse() =>
        Assert.False(ParserLineaDeudor.TryParse(LineaNormal[..^1], out _));

    [Fact]
    public void TryParse_SituacionNoNumerica_DevuelveFalse() =>
        Assert.False(ParserLineaDeudor.TryParse(LineaNormal[..27] + "X " + LineaNormal[29..], out _));
}
