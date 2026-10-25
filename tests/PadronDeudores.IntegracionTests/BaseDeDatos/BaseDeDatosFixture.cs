using Npgsql;
using PadronDeudores.Migraciones;
using Testcontainers.PostgreSql;

namespace PadronDeudores.IntegracionTests.BaseDeDatos;

// Levanta un PostgreSQL real en Docker, crea los usuarios y aplica las migraciones.
public sealed class BaseDeDatosFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _contenedor = new PostgreSqlBuilder("postgres:17.10").Build();

    public NpgsqlDataSource BaseDeDatos { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _contenedor.StartAsync();
        BaseDeDatos = NpgsqlDataSource.Create(_contenedor.GetConnectionString());

        // En los tests no corre 01-usuarios.sh: creamos los usuarios acá para que la migración pueda darles permisos.
        await using (var usuarios = BaseDeDatos.CreateCommand("CREATE ROLE importador LOGIN; CREATE ROLE consulta LOGIN;"))
        {
            await usuarios.ExecuteNonQueryAsync();
        }

        Assert.True(Migrador.Aplicar(_contenedor.GetConnectionString()));
    }

    public async Task DisposeAsync()
    {
        await BaseDeDatos.DisposeAsync();
        await _contenedor.DisposeAsync();
    }
}

[CollectionDefinition(Nombre)]
public sealed class ColeccionBaseDeDatos : ICollectionFixture<BaseDeDatosFixture>
{
    public const string Nombre = "Base de datos";
}
