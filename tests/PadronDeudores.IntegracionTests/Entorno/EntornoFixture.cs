using Amazon.Runtime;
using Amazon.SQS;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using PadronDeudores.Dominio;
using PadronDeudores.Importador.Worker.Procesamiento;
using PadronDeudores.Infraestructura.Archivos;
using PadronDeudores.Infraestructura.Persistencia;
using PadronDeudores.Migraciones;
using Testcontainers.LocalStack;
using Testcontainers.PostgreSql;

namespace PadronDeudores.IntegracionTests.Entorno;

// Levanta un PostgreSQL real en Docker, crea los usuarios y aplica las migraciones. También levanta LocalStack con la cola.
public sealed class EntornoFixture : IAsyncLifetime
{
    public const string Region = "us-east-1";
    public const string Cola = "importaciones";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17.10").Build();
    private readonly LocalStackContainer _localStack = new LocalStackBuilder("localstack/localstack:4.14.0").Build();

    public NpgsqlDataSource BaseDeDatos { get; private set; } = null!;
    public IAmazonSQS Sqs { get; private set; } = null!;
    public string CadenaConexion => _postgres.GetConnectionString();
    public string UrlLocalStack => _localStack.GetConnectionString();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _localStack.StartAsync());
        BaseDeDatos = NpgsqlDataSource.Create(CadenaConexion);

        // En los tests no corre 01-usuarios.sh: creamos los usuarios acá para que la migración pueda darles permisos.
        await using (var usuarios = BaseDeDatos.CreateCommand("CREATE ROLE importador LOGIN; CREATE ROLE consulta LOGIN;"))
        {
            await usuarios.ExecuteNonQueryAsync();
        }

        Assert.True(Migrador.Aplicar(CadenaConexion));

        // Las APIs toman las credenciales de AWS del entorno, como en Docker.
        Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "test");
        Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "test");
        Sqs = new AmazonSQSClient(new BasicAWSCredentials("test", "test"),
            new AmazonSQSConfig { ServiceURL = UrlLocalStack, AuthenticationRegion = Region });
        await Sqs.CreateQueueAsync(Cola);
    }

    public ProcesadorImportacion CrearProcesador() => new(
        BaseDeDatos,
        new OrigenDisco(),
        new LectorDeudores(NullLogger<LectorDeudores>.Instance),
        new CargadorStaging(BaseDeDatos),
        new ConsolidadorResumenes(),
        new RepositorioImportaciones(BaseDeDatos),
        NullLogger<ProcesadorImportacion>.Instance);

    // Lo mismo que hace la API al recibir el pedido: registrarlo como pendiente.
    public async Task<Guid> RegistrarImportacionAsync(string archivo)
    {
        var id = Guid.CreateVersion7();
        await new RepositorioImportaciones(BaseDeDatos).RegistrarAsync(id, OrigenImportacion.Ruta, archivo, CancellationToken.None);
        return id;
    }

    public async Task DisposeAsync()
    {
        Sqs.Dispose();
        await BaseDeDatos.DisposeAsync();
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _localStack.DisposeAsync().AsTask());
    }
}

[CollectionDefinition(Nombre)]
public sealed class ColeccionEntorno : ICollectionFixture<EntornoFixture>
{
    public const string Nombre = "Entorno";
}
