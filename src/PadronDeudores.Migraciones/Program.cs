using PadronDeudores.Migraciones;

// Aplica las migraciones pendientes y termina. Código de salida 0 = ok, 1 = error.
var cadenaConexion = Environment.GetEnvironmentVariable("ConnectionStrings__Importador")
    ?? throw new InvalidOperationException("Falta la variable ConnectionStrings__Importador.");

return Migrador.Aplicar(cadenaConexion) ? 0 : 1;
