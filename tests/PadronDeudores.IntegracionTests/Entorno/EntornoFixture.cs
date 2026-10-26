using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
using PadronDeudores.Dominio;
using PadronDeudores.Importador.Worker.Notificaciones;
using PadronDeudores.Importador.Worker.Procesamiento;
using PadronDeudores.Infraestructura.Archivos;
using PadronDeudores.Infraestructura.Persistencia;
using PadronDeudores.IntegracionTests.Notificaciones;
using PadronDeudores.Migraciones;
using Testcontainers.LocalStack;
using Testcontainers.PostgreSql;

namespace PadronDeudores.IntegracionTests.Entorno;

// Levanta un PostgreSQL real en Docker, crea los usuarios y aplica las migraciones. También levanta LocalStack con la cola.
public sealed class EntornoFixture : IAsyncLifetime
{
    public const string Region = "us-east-1";
    public const string Cola = "importaciones";
    public const string Bucket = "padron-archivos";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17.11").Build();
    private readonly LocalStackContainer _localStack = new LocalStackBuilder("localstack/localstack:4.14.0").Build();

    public NpgsqlDataSource BaseDeDatos { get; private set; } = null!;
    public IAmazonSQS Sqs { get; private set; } = null!;
    public IAmazonS3 S3 { get; private set; } = null!;
    public string CadenaConexion => _postgres.GetConnectionString();
    public string UrlLocalStack => _localStack.GetConnectionString();

    // La misma base, con el usuario de solo lectura que usa la API de consulta.
    public string CadenaConexionConsulta =>
        new NpgsqlConnectionStringBuilder(CadenaConexion) { Username = "consulta", Password = "consulta" }.ConnectionString;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _localStack.StartAsync());
        BaseDeDatos = NpgsqlDataSource.Create(CadenaConexion);

        // En los tests no corre 01-usuarios.sh: creamos los usuarios acá para que la migración pueda darles permisos.
        await using (var usuarios = BaseDeDatos.CreateCommand("CREATE ROLE importador LOGIN PASSWORD 'importador'; CREATE ROLE consulta LOGIN PASSWORD 'consulta';"))
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
        S3 = new AmazonS3Client(new BasicAWSCredentials("test", "test"),
            new AmazonS3Config { ServiceURL = UrlLocalStack, AuthenticationRegion = Region, ForcePathStyle = true });
        await S3.PutBucketAsync(Bucket);
    }

    // Lugares permitidos en los tests: la carpeta Muestras y el bucket de test.
    public OrigenSegunArchivo CrearOrigen() => new(new OrigenDisco(), new ArchivosS3(S3), Options.Create(
        new OpcionesArchivos { DirectorioArchivos = Path.GetFullPath("Muestras"), BucketArchivos = Bucket }));

    public ProcesadorImportacion CrearProcesador(INotificador? notificador = null) => new(
        BaseDeDatos,
        CrearOrigen(),
        new LectorDeudores(NullLogger<LectorDeudores>.Instance),
        new CargadorStaging(BaseDeDatos),
        new ConsolidadorResumenes(),
        new RepositorioImportaciones(BaseDeDatos),
        notificador ?? new NotificadorEnMemoria(),
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
        S3.Dispose();
        await BaseDeDatos.DisposeAsync();
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _localStack.DisposeAsync().AsTask());
    }
}

[CollectionDefinition(Nombre)]
public sealed class ColeccionEntorno : ICollectionFixture<EntornoFixture>
{
    public const string Nombre = "Entorno";
}
