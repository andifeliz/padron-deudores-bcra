using PadronDeudores.Dominio;

namespace PadronDeudores.Dominio.Tests;

public class NivelDeRiesgoTests
{
    // Códigos que no siguen la escala 1 a 5
    [Theory]
    [InlineData(11, 1)]
    [InlineData(21, 2)]
    [InlineData(22, 2)]
    [InlineData(23, 2)]
    [InlineData(99, 0)]
    public void De_CodigoEspecial_DevuelveSuNivelReal(short codigo, short nivelEsperado) =>
        Assert.Equal(nivelEsperado, NivelDeRiesgo.De(codigo));

    [Fact]
    public void De_CodigoNoDocumentado_DevuelveNull() =>
        Assert.Null(NivelDeRiesgo.De(7));
}
