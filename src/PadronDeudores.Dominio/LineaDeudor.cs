namespace PadronDeudores.Dominio;

// Campos de una línea de deudores.txt que usa el proceso. Los préstamos están en miles de pesos.
public readonly record struct LineaDeudor(
    string CodigoEntidad,
    string Periodo,
    string NroIdentificacion,
    short Situacion,
    decimal Prestamos);
