using System.Reflection;
using DbUp;

namespace PadronDeudores.Migraciones;

// Aplica las migraciones pendientes (scripts SQL incluidos en el proyecto), cada una en su transacción.
public static class Migrador
{
    public static bool Aplicar(string cadenaConexion) =>
        DeployChanges.To
            .PostgresqlDatabase(cadenaConexion)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .WithTransactionPerScript()
            .LogToConsole()
            .Build()
            .PerformUpgrade()
            .Successful;
}
