using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using PadronDeudores.Infraestructura.Archivos;
using PadronDeudores.Infraestructura.Mensajeria;

namespace PadronDeudores.Infraestructura;

// Registro de las conexiones externas, compartido por los servicios.
public static class Configuracion
{
    // Un único NpgsqlDataSource por servicio: guarda conexiones abiertas y las reutiliza entre pedidos.
    public static void AgregarBaseDeDatos(this IHostApplicationBuilder builder, string nombreConexion)
    {
        var cadena = builder.Configuration.GetConnectionString(nombreConexion)
            ?? throw new InvalidOperationException($"Falta la conexión ConnectionStrings:{nombreConexion}.");

        // No usamos Kerberos: sin esto, Npgsql busca libgssapi_krb5 (que la imagen no trae) y escribe un error falso en el log.
        var conexion = new NpgsqlConnectionStringBuilder(cadena) { GssEncryptionMode = GssEncryptionMode.Disable };
        builder.Services.AddSingleton(NpgsqlDataSource.Create(conexion.ConnectionString));
    }

    // Las credenciales de AWS se toman del entorno (AWS_ACCESS_KEY_ID y AWS_SECRET_ACCESS_KEY).
    public static void AgregarColaImportaciones(this IHostApplicationBuilder builder)
    {
        builder.Services.AgregarOpciones<OpcionesAws>(OpcionesAws.Seccion);
        builder.Services.AddSingleton<IAmazonSQS>(sp => new AmazonSQSClient(Configurar(new AmazonSQSConfig(), Aws(sp))));
        builder.Services.AddSingleton<ColaImportaciones>();
    }

    // ForcePathStyle: LocalStack recibe el bucket en la ruta (localstack:4566/bucket) y no como subdominio.
    public static void AgregarArchivosS3(this IHostApplicationBuilder builder)
    {
        builder.Services.AgregarOpciones<OpcionesAws>(OpcionesAws.Seccion);
        builder.Services.AddSingleton<IAmazonS3>(sp => new AmazonS3Client(Configurar(new AmazonS3Config { ForcePathStyle = true }, Aws(sp))));
        builder.Services.AddSingleton<ArchivosS3>();
    }

    // Lee la sección de configuración y la valida al arrancar: si falta algo, el servicio no levanta.
    public static void AgregarOpciones<T>(this IServiceCollection services, string seccion) where T : class =>
        services.AddOptions<T>().BindConfiguration(seccion).ValidateDataAnnotations().ValidateOnStart();

    private static OpcionesAws Aws(IServiceProvider servicios) => servicios.GetRequiredService<IOptions<OpcionesAws>>().Value;

    // Con LocalStack, su dirección; en AWS real, la región.
    private static T Configurar<T>(T config, OpcionesAws aws) where T : ClientConfig
    {
        if (string.IsNullOrEmpty(aws.ServiceUrl))
        {
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(aws.Region);
        }
        else
        {
            config.ServiceURL = aws.ServiceUrl;
            config.AuthenticationRegion = aws.Region;
        }

        return config;
    }
}
