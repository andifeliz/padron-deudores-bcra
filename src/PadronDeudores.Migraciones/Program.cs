using System.Reflection;
using DbUp;

// Aplica las migraciones pendientes y termina. Código de salida 0 = ok, 1 = error.
var cadenaConexion = Environment.GetEnvironmentVariable("ConnectionStrings__Importador")
    ?? throw new InvalidOperationException("Falta la variable ConnectionStrings__Importador.");

var resultado = DeployChanges.To
    .PostgresqlDatabase(cadenaConexion)
    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
    .WithTransactionPerScript()
    .LogToConsole()
    .Build()
    .PerformUpgrade();

return resultado.Successful ? 0 : 1;
