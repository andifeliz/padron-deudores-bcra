namespace PadronDeudores.Infraestructura.Persistencia;

// Resumen de una entidad. La suma está en miles de pesos, como en el archivo del BCRA.
public sealed record Entidad(string CodigoEntidad, decimal SumaTotalPrestamos);
