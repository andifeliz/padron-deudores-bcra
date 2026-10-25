namespace PadronDeudores.Dominio;

// Traduce un código de situación a su nivel de riesgo, para comparar códigos que no siguen la escala 1 a 5
public static class NivelDeRiesgo
{
    // Devuelve null si el código no está documentado: no compite con los conocidos.
    public static short? De(short codigoSituacion) => (Situacion)codigoSituacion switch
    {
        Situacion.NoClasificado => 0,
        Situacion.Normal or Situacion.CubiertaConGarantiaA => 1,
        Situacion.RiesgoBajo or Situacion.EnObservacion or Situacion.EnNegociacion or Situacion.TratamientoEspecial => 2,
        Situacion.RiesgoMedio => 3,
        Situacion.RiesgoAlto => 4,
        Situacion.Irrecuperable => 5,
        _ => null,
    };
}
