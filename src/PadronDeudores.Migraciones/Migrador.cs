using System.Reflection;
using DbUp;
using Npgsql;

namespace PadronDeudores.Migraciones;

// Aplica las migraciones pendientes (scripts SQL incluidos en el proyecto), cada una en su transacción.
public static class Migrador
{
    public static bool Aplicar(string cadenaConexion) =>
        DeployChanges.To
            .PostgresqlDatabase(SinKerberos(cadenaConexion))
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .WithTransactionPerScript()
            .LogToConsole()
            .Build()
            .PerformUpgrade()
            .Successful;

    // No usamos Kerberos: sin esto, Npgsql busca libgssapi_krb5 (que la imagen no trae) y escribe un error falso en el log.
    private static string SinKerberos(string cadenaConexion) =>
        new NpgsqlConnectionStringBuilder(cadenaConexion) { GssEncryptionMode = GssEncryptionMode.Disable }.ConnectionString;
}
