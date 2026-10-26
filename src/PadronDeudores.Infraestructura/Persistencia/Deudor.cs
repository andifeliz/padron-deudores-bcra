namespace PadronDeudores.Infraestructura.Persistencia;

// Resumen de un deudor. La suma está en miles de pesos, como en el archivo del BCRA.
public sealed record Deudor(string NroIdentificacion, short SituacionMaxima, decimal SumaTotalPrestamos);
