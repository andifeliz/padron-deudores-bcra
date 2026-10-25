namespace PadronDeudores.Dominio;

// Códigos de situación del BCRA
public enum Situacion : short
{
    Normal = 1,
    RiesgoBajo = 2,
    RiesgoMedio = 3,
    RiesgoAlto = 4,
    Irrecuperable = 5,
    CubiertaConGarantiaA = 11,
    EnObservacion = 21,
    EnNegociacion = 22,
    TratamientoEspecial = 23,
    NoClasificado = 99,
}
